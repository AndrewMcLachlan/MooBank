#nullable enable
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Services;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

/// <summary>
/// Unit tests for measuring planned items against the spending that actually carried their tags.
/// </summary>
/// <remarks>
/// The cases here are the ones issue #928 named: a planned expense turning up in the transaction
/// log throws out the expense calculations "even though it was expected", and the payment may not
/// match the plan in amount, in timing, or in being a single payment at all.
/// </remarks>
[Trait("Category", "Unit")]
public class PlannedItemRealiserTests
{
    private const int SolarTag = 42;
    private const int SchoolFeesTag = 77;

    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid SavingsId = Guid.NewGuid();

    private static readonly DateOnly PlanStart = new(2026, 1, 1);
    private static readonly DateOnly PlanEnd = new(2026, 12, 31);

    private static DomainForecastPlan Plan(params DomainPlannedItem[] items)
    {
        var plan = new DomainForecastPlan(Guid.NewGuid())
        {
            Name = "Test Plan",
            FamilyId = Guid.NewGuid(),
            StartDate = PlanStart,
            EndDate = PlanEnd,
        };

        foreach (var item in items)
        {
            item.ForecastPlanId = plan.Id;
            plan.PlannedItems.Add(item);
        }

        return plan;
    }

    private static DomainPlannedItem OneOff(string name, decimal amount, DateOnly on, int? tagId = SolarTag) =>
        new(Guid.NewGuid())
        {
            Name = name,
            ItemType = PlannedItemType.Expense,
            Amount = amount,
            TagId = tagId,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = on },
        };

    private static DomainPlannedItem Recurring(string name, decimal amount, DateOnly from, int? tagId = SolarTag) =>
        new(Guid.NewGuid())
        {
            Name = name,
            ItemType = PlannedItemType.Expense,
            Amount = amount,
            TagId = tagId,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Monthly, AnchorDate = from, Interval = 1 },
        };

    private static TaggedSpend Spend(DateOnly month, decimal amount, int tagId = SolarTag, Guid? account = null, bool inReporting = true) =>
        new(account ?? AccountId, month, tagId, TransactionType.Debit, amount, inReporting);

    private static RealisedPlan Realise(DomainForecastPlan plan, DateOnly settledThrough, params TaggedSpend[] spend) =>
        PlannedItemRealiser.Realise(plan, spend, [AccountId], settledThrough, slippageMonths: 1);

    private static decimal Month(Dictionary<string, decimal> byMonth, int year, int month) =>
        byMonth.GetValueOrDefault(new DateOnly(year, month, 1).ToString("yyyy-MM"), 0m);

    /// <summary>
    /// Given a planned expense that came in higher than planned
    /// When the plan is realised
    /// Then the settled month should carry what was actually spent
    /// </summary>
    [Fact]
    public void Realise_SpentMoreThanPlanned_TheMonthCarriesWhatWasSpent()
    {
        var plan = Plan(OneOff("Solar", 15_000m, new DateOnly(2026, 3, 15)));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Spend(new DateOnly(2026, 3, 1), 17_238.40m));

        Assert.Equal(17_238.40m, Month(realised.ExpensesByMonth, 2026, 3));
    }

    /// <summary>
    /// Given a planned expense paid a month later than planned
    /// When the plan is realised
    /// Then the planned month should be empty and the month it was paid should carry it
    /// </summary>
    [Fact]
    public void Realise_PaidLate_TheMoneyMovesToTheMonthItWasPaid()
    {
        var plan = Plan(OneOff("Fence", 18_600m, new DateOnly(2026, 3, 15)));

        var realised = Realise(plan, new DateOnly(2026, 5, 1), Spend(new DateOnly(2026, 4, 1), 18_600m));

        Assert.Equal(0m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(18_600m, Month(realised.ExpensesByMonth, 2026, 4));
    }

    /// <summary>
    /// Given a one-off paid across several months, as building work is
    /// When the plan is realised
    /// Then each month should carry its own share
    /// </summary>
    [Fact]
    public void Realise_PaidAcrossSeveralMonths_EachMonthCarriesItsOwnShare()
    {
        var plan = Plan(OneOff("Renovation", 15_000m, new DateOnly(2026, 2, 15)));

        var realised = Realise(
            plan,
            new DateOnly(2026, 4, 1),
            Spend(new DateOnly(2026, 2, 1), 4_000m),
            Spend(new DateOnly(2026, 3, 1), 6_000m));

        Assert.Equal(4_000m, Month(realised.ExpensesByMonth, 2026, 2));
        Assert.Equal(6_000m, Month(realised.ExpensesByMonth, 2026, 3));
    }

    /// <summary>
    /// Given a one-off part paid, whose planned date has passed
    /// When the plan is realised
    /// Then the outstanding balance should move to the next month rather than disappear
    /// </summary>
    /// <remarks>
    /// A one-off has a single month of its own, so once its date is behind us there is nowhere left
    /// to re-spread the remainder — and dropping it would quietly make the forecast optimistic by
    /// exactly the amount still owed.
    /// </remarks>
    [Fact]
    public void Realise_PartPaidAndItsDatePassed_TheRemainderMovesToTheNextMonth()
    {
        var plan = Plan(OneOff("Renovation", 15_000m, new DateOnly(2026, 3, 15)));

        var realised = Realise(plan, new DateOnly(2026, 3, 1), Spend(new DateOnly(2026, 3, 1), 8_000m));

        Assert.Equal(8_000m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(7_000m, Month(realised.ExpensesByMonth, 2026, 4));
    }

    /// <summary>
    /// Given a one-off that came in slightly under, whose claim window has closed
    /// When the plan is realised
    /// Then the shortfall should be written off rather than carried forever
    /// </summary>
    [Fact]
    public void Realise_CameInUnderAndTheWindowClosed_TheShortfallIsWrittenOff()
    {
        var plan = Plan(OneOff("Rates", 200m, new DateOnly(2026, 2, 15)));

        // Settled through April: February's window (±1 month) closed at the end of March.
        var realised = Realise(plan, new DateOnly(2026, 4, 1), Spend(new DateOnly(2026, 2, 1), 195m));

        Assert.Equal(195m, Month(realised.ExpensesByMonth, 2026, 2));
        Assert.Equal(0m, Month(realised.ExpensesByMonth, 2026, 5));
        Assert.Equal(0m, realised.ExpensesByMonth.Values.Sum() - 195m);

        var progress = Assert.Single(realised.Progress);
        Assert.True(progress.IsClosed);
        Assert.Equal(195m, progress.ActualToDate);
    }

    /// <summary>
    /// Given a planned expense that never happened, whose window has closed
    /// When the plan is realised
    /// Then it should contribute nothing and be reported closed
    /// </summary>
    [Fact]
    public void Realise_NeverHappened_ContributesNothingAndIsReportedClosed()
    {
        var plan = Plan(OneOff("Aircon", 14_000m, new DateOnly(2026, 2, 15)));

        var realised = Realise(plan, new DateOnly(2026, 6, 1));

        Assert.DoesNotContain(realised.ExpensesByMonth, m => m.Value != 0m);

        var progress = Assert.Single(realised.Progress);
        Assert.Equal(0m, progress.ActualToDate);
        Assert.True(progress.IsClosed);
        Assert.True(progress.IsMatched);
    }

    /// <summary>
    /// Given an item with no tag
    /// When the plan is realised
    /// Then it should stand exactly as planned
    /// </summary>
    /// <remarks>
    /// Realisation is opt-in per item, so a plan whose items carry no tags forecasts exactly as it
    /// did before any of this existed.
    /// </remarks>
    [Fact]
    public void Realise_UntaggedItem_StandsAsPlanned()
    {
        var plan = Plan(OneOff("Holiday", 8_000m, new DateOnly(2026, 3, 15), tagId: null));

        // Spending exists in the same month, but with nothing to attach it to.
        var realised = Realise(plan, new DateOnly(2026, 6, 1), Spend(new DateOnly(2026, 3, 1), 5_000m));

        Assert.Equal(8_000m, Month(realised.ExpensesByMonth, 2026, 3));

        var progress = Assert.Single(realised.Progress);
        Assert.False(progress.IsMatched);
        Assert.Equal(0m, progress.ActualToDate);
    }

    /// <summary>
    /// Given two items sharing one tag
    /// When a month's spending is claimed by both
    /// Then it should be split in proportion to what each planned
    /// </summary>
    /// <remarks>
    /// A real shape, not a contrivance: one school fees item per child, both against a single
    /// "School Fees" tag.
    /// </remarks>
    [Fact]
    public void Realise_TwoItemsShareATag_TheMonthIsSplitInProportion()
    {
        var felix = OneOff("Felix School Fees", 22_000m, new DateOnly(2026, 2, 15), SchoolFeesTag);
        var xander = OneOff("Xander School Fees", 11_000m, new DateOnly(2026, 2, 15), SchoolFeesTag);
        var plan = Plan(felix, xander);

        var realised = Realise(plan, new DateOnly(2026, 3, 1), Spend(new DateOnly(2026, 2, 1), 30_000m, SchoolFeesTag));

        // 22,000 : 11,000 is two to one.
        var felixProgress = realised.Progress.Single(p => p.PlannedItemId == felix.Id);
        var xanderProgress = realised.Progress.Single(p => p.PlannedItemId == xander.Id);

        Assert.Equal(20_000m, felixProgress.ActualToDate);
        Assert.Equal(10_000m, xanderProgress.ActualToDate);
        Assert.Equal(30_000m, Month(realised.ExpensesByMonth, 2026, 2));
    }

    /// <summary>
    /// Given spending that falls outside an item's claim window
    /// When the plan is realised
    /// Then it should be left as ordinary spending
    /// </summary>
    /// <remarks>
    /// Matching is bounded rather than eager. Keeping the plan close to reality is the author's job;
    /// the engine's is to make the divergence visible, not to go hunting for a payment that might
    /// plausibly have been this item.
    /// </remarks>
    [Fact]
    public void Realise_SpendingOutsideTheWindow_IsLeftAsOrdinarySpending()
    {
        var plan = Plan(OneOff("Solar", 17_238.40m, new DateOnly(2026, 2, 15)));

        // Six months after the planned date, well beyond the one-month allowance.
        var realised = Realise(plan, new DateOnly(2026, 9, 1), Spend(new DateOnly(2026, 8, 1), 17_238.40m));

        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
        Assert.Equal(0m, Assert.Single(realised.Progress).ActualToDate);
    }

    /// <summary>
    /// Given a recurring charge
    /// When a settled month is realised
    /// Then it should carry the actual, and future months should still carry the planned amount
    /// </summary>
    /// <remarks>
    /// A recurring charge is never used up: paying this month's electricity does nothing about next
    /// month's, so the remaining months must not be reduced by what has already been paid.
    /// </remarks>
    [Fact]
    public void Realise_RecurringCharge_IsNeverUsedUp()
    {
        var plan = Plan(Recurring("Electricity", 300m, PlanStart));

        var realised = Realise(
            plan,
            new DateOnly(2026, 2, 1),
            Spend(new DateOnly(2026, 1, 1), 355m),
            Spend(new DateOnly(2026, 2, 1), 410m));

        Assert.Equal(355m, Month(realised.ExpensesByMonth, 2026, 1));
        Assert.Equal(410m, Month(realised.ExpensesByMonth, 2026, 2));
        Assert.Equal(300m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(300m, Month(realised.ExpensesByMonth, 2026, 12));
    }

    /// <summary>
    /// Given spending on an account outside the historical-analysis set
    /// When the plan is realised
    /// Then it should count towards the item but not towards the baseline subtraction
    /// </summary>
    /// <remarks>
    /// A car paid for out of savings is still the car, so it realises the item. But the baseline it
    /// would otherwise be subtracted from was never computed over savings accounts, so subtracting
    /// it there would take out spending that was never in the figure.
    /// </remarks>
    [Fact]
    public void Realise_PaidFromASavingsAccount_RealisesTheItemButNotTheBaseline()
    {
        var plan = Plan(OneOff("New Car", 50_000m, new DateOnly(2026, 3, 15)));

        var realised = PlannedItemRealiser.Realise(
            plan,
            [Spend(new DateOnly(2026, 3, 1), 50_000m, account: SavingsId)],
            historicalAccountIds: [AccountId],
            latestTransactionMonth: new DateOnly(2026, 4, 1),
            slippageMonths: 1);

        Assert.Equal(50_000m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(50_000m, Assert.Single(realised.Progress).ActualToDate);

        // Nothing to take out of a baseline that never saw it.
        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
    }

    /// <summary>
    /// Given a payment marked as excluded from reporting
    /// When the plan is realised
    /// Then it should still pay off the item, but not be taken back out of the baseline
    /// </summary>
    /// <remarks>
    /// Keeping a large one-off out of the reports is the same instinct as planning for it, so these
    /// are exactly the payments a planned item is most likely to be waiting for — the solar
    /// installation this was found on was marked that way. The money left the account, so the item
    /// is paid.
    ///
    /// The baseline is the other way round. Both the lookback average and the regression's training
    /// data are built from procedures that skip these transactions, so this spending was never in
    /// them; subtracting it would take out something that was never there and understate ordinary
    /// spending by the whole amount.
    /// </remarks>
    [Fact]
    public void Realise_PaymentExcludedFromReporting_PaysTheItemButIsNotTakenOutOfTheBaseline()
    {
        var plan = Plan(OneOff("Solar", 17_238.40m, new DateOnly(2026, 3, 6)));

        var realised = Realise(
            plan,
            new DateOnly(2026, 4, 1),
            Spend(new DateOnly(2026, 3, 1), 17_238.40m, inReporting: false));

        Assert.Equal(17_238.40m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(17_238.40m, Assert.Single(realised.Progress).ActualToDate);

        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
    }

    /// <summary>
    /// Given a payment that reporting does count
    /// When the plan is realised
    /// Then it should be taken back out of the baseline
    /// </summary>
    [Fact]
    public void Realise_PaymentVisibleToReporting_IsTakenOutOfTheBaseline()
    {
        var plan = Plan(OneOff("Solar", 17_238.40m, new DateOnly(2026, 3, 6)));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Spend(new DateOnly(2026, 3, 1), 17_238.40m));

        Assert.Equal(17_238.40m, Month(realised.AttributedByMonth, 2026, 3));
    }

    /// <summary>
    /// Given a planned income item with a tag
    /// When the plan is realised
    /// Then only credits should be attributed to it
    /// </summary>
    [Fact]
    public void Realise_IncomeItem_ClaimsCreditsAndNotDebits()
    {
        var salary = new DomainPlannedItem(Guid.NewGuid())
        {
            Name = "Salary",
            ItemType = PlannedItemType.Income,
            Amount = 5_000m,
            TagId = SolarTag,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Monthly, AnchorDate = PlanStart, Interval = 1 },
        };

        var realised = Realise(
            plan: Plan(salary),
            settledThrough: new DateOnly(2026, 1, 1),
            new TaggedSpend(AccountId, new DateOnly(2026, 1, 1), SolarTag, TransactionType.Credit, 5_600m, true),
            new TaggedSpend(AccountId, new DateOnly(2026, 1, 1), SolarTag, TransactionType.Debit, 999m, true));

        Assert.Equal(5_600m, Month(realised.IncomeByMonth, 2026, 1));

        // An income item never contributes to the expense series, whatever carries its tag.
        Assert.DoesNotContain(realised.ExpensesByMonth, m => m.Value != 0m);
        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
    }
}
