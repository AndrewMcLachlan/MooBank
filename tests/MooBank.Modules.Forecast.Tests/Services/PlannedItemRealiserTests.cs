#nullable enable
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Services;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

/// <summary>
/// Unit tests for measuring planned expenses against the payments linked to them.
/// </summary>
/// <remarks>
/// The cases here are the ones issue #928 named: a planned expense turning up in the transaction
/// log throws out the expense calculations "even though it was expected", and the payment may not
/// match the plan in amount, in timing, or in being a single payment at all.
/// </remarks>
[Trait("Category", "Unit")]
public class PlannedItemRealiserTests
{
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

    private static DomainPlannedItem OneOff(string name, decimal amount, DateOnly on) =>
        new(Guid.NewGuid())
        {
            Name = name,
            ItemType = PlannedItemType.Expense,
            Amount = amount,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = on },
        };

    private static DomainPlannedItem Recurring(string name, decimal amount, DateOnly from, ScheduleFrequency frequency = ScheduleFrequency.Monthly) =>
        new(Guid.NewGuid())
        {
            Name = name,
            ItemType = PlannedItemType.Expense,
            Amount = amount,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = frequency, AnchorDate = from, Interval = 1 },
        };

    private static DomainPlannedItem LinkedTo(DomainPlannedItem item, params Guid[] transactionIds)
    {
        foreach (var id in transactionIds)
        {
            item.Transactions.Add(new ForecastPlannedItemTransaction(Guid.NewGuid())
            {
                PlannedItemId = item.Id,
                TransactionId = id,
            });
        }

        return item;
    }

    private static LinkedPayment Payment(Guid transactionId, DateOnly month, decimal amount, bool inReporting = true, Guid? account = null) =>
        new(transactionId, account ?? AccountId, month, amount, inReporting);

    private static RealisedPlan Realise(DomainForecastPlan plan, DateOnly settledThrough, params LinkedPayment[] payments) =>
        PlannedItemRealiser.Realise(plan, payments, [AccountId], settledThrough);

    private static decimal Month(Dictionary<string, decimal> byMonth, int year, int month) =>
        byMonth.GetValueOrDefault(new DateOnly(year, month, 1).ToString("yyyy-MM"), 0m);

    /// <summary>
    /// Given an item with no linked payments
    /// When the plan is realised
    /// Then it should stand exactly as planned
    /// </summary>
    /// <remarks>
    /// Realisation is opt-in. A plan whose items nobody has linked forecasts precisely as it did
    /// before any of this existed, which is what makes the feature safe to adopt gradually.
    /// </remarks>
    [Fact]
    public void Realise_ItemWithNoLinks_StandsAsPlanned()
    {
        var plan = Plan(OneOff("Holiday", 8_000m, new DateOnly(2026, 3, 15)));

        var realised = Realise(plan, new DateOnly(2026, 6, 1));

        Assert.Equal(8_000m, Month(realised.ExpensesByMonth, 2026, 3));

        var progress = Assert.Single(realised.Progress);
        Assert.False(progress.IsMatched);
        Assert.Equal(0m, progress.ActualToDate);
    }

    /// <summary>
    /// Given a linked payment larger than the item planned for
    /// When the plan is realised
    /// Then the month should carry what was actually paid
    /// </summary>
    [Fact]
    public void Realise_PaidMoreThanPlanned_TheMonthCarriesWhatWasPaid()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Solar", 15_000m, new DateOnly(2026, 3, 15)), id));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Payment(id, new DateOnly(2026, 3, 1), 17_238.40m));

        Assert.Equal(17_238.40m, Month(realised.ExpensesByMonth, 2026, 3));
    }

    /// <summary>
    /// Given a payment made later than the item planned
    /// When the plan is realised
    /// Then the money should sit in the month it was actually paid
    /// </summary>
    [Fact]
    public void Realise_PaidLate_TheMoneySitsInTheMonthItWasPaid()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Fence", 18_600m, new DateOnly(2026, 3, 15)), id));

        var realised = Realise(plan, new DateOnly(2026, 5, 1), Payment(id, new DateOnly(2026, 4, 1), 18_600m));

        Assert.Equal(0m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(18_600m, Month(realised.ExpensesByMonth, 2026, 4));
    }

    /// <summary>
    /// Given a job paid in instalments
    /// When the plan is realised
    /// Then each month should carry its own instalment
    /// </summary>
    [Fact]
    public void Realise_PaidInInstalments_EachMonthCarriesItsOwn()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Renovation", 15_000m, new DateOnly(2026, 2, 15)), first, second));

        var realised = Realise(
            plan,
            new DateOnly(2026, 4, 1),
            Payment(first, new DateOnly(2026, 2, 1), 4_000m),
            Payment(second, new DateOnly(2026, 3, 1), 6_000m));

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
    /// to re-spread the remainder -- and dropping it would quietly make the forecast optimistic by
    /// exactly the amount still owed.
    /// </remarks>
    [Fact]
    public void Realise_PartPaidAndItsDatePassed_TheRemainderMovesToTheNextMonth()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Renovation", 15_000m, new DateOnly(2026, 3, 15)), id));

        var realised = Realise(plan, new DateOnly(2026, 3, 1), Payment(id, new DateOnly(2026, 3, 1), 8_000m));

        Assert.Equal(8_000m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(7_000m, Month(realised.ExpensesByMonth, 2026, 4));
    }

    /// <summary>
    /// Given a recurring charge with a linked payment
    /// When the plan is realised
    /// Then future months should still carry the planned amount
    /// </summary>
    /// <remarks>
    /// A recurring charge is never used up: paying this month's electricity does nothing about next
    /// month's, so the remaining months must not be reduced by what has already been paid.
    /// </remarks>
    [Fact]
    public void Realise_RecurringCharge_IsNeverUsedUp()
    {
        var january = Guid.NewGuid();
        var plan = Plan(LinkedTo(Recurring("Electricity", 300m, PlanStart), january));

        var realised = Realise(plan, new DateOnly(2026, 1, 1), Payment(january, new DateOnly(2026, 1, 1), 355m));

        Assert.Equal(355m, Month(realised.ExpensesByMonth, 2026, 1));
        Assert.Equal(300m, Month(realised.ExpensesByMonth, 2026, 2));
        Assert.Equal(300m, Month(realised.ExpensesByMonth, 2026, 12));
    }

    /// <summary>
    /// Given a recurring item with one occurrence linked
    /// When the plan is realised
    /// Then the occurrences that were not linked should stand as planned
    /// </summary>
    /// <remarks>
    /// The defect this pins down, from the real data. School fees are a yearly item, and linking a
    /// single year emptied every other year: settled months were taken to have cost only what had
    /// been linked to them, so a year nobody had got round to linking read as nothing spent, and a
    /// real twenty-one thousand pound payment vanished from the projection the moment the forecast
    /// was told about a different one.
    ///
    /// No link is absence of information, not evidence of nought. Each occurrence answers for
    /// itself: linked, and it is what was paid; not linked, and it stands as planned.
    /// </remarks>
    [Fact]
    public void Realise_RecurringItemWithOneOccurrenceLinked_LeavesTheRestAsPlanned()
    {
        var marchPayment = Guid.NewGuid();
        var fees = LinkedTo(Recurring("School Fees", 21_000m, PlanStart), marchPayment);

        // Settled through June, with only March's payment linked.
        var realised = Realise(Plan(fees), new DateOnly(2026, 6, 1), Payment(marchPayment, new DateOnly(2026, 3, 1), 22_500m));

        // The linked month is what was actually paid.
        Assert.Equal(22_500m, Month(realised.ExpensesByMonth, 2026, 3));

        // The settled months nobody linked still stand as planned. Reading them as nought is what
        // made a real payment disappear from the projection.
        Assert.Equal(21_000m, Month(realised.ExpensesByMonth, 2026, 1));
        Assert.Equal(21_000m, Month(realised.ExpensesByMonth, 2026, 2));
        Assert.Equal(21_000m, Month(realised.ExpensesByMonth, 2026, 4));

        // And so do the months still ahead.
        Assert.Equal(21_000m, Month(realised.ExpensesByMonth, 2026, 12));
    }

    /// <summary>
    /// Given a recurring payment made a few days before the month it was due
    /// When the plan is realised
    /// Then it should settle that occurrence rather than be counted alongside it
    /// </summary>
    /// <remarks>
    /// The defect this pins down, from the real data. School fees fall due in February; the 2025
    /// invoice was paid on 29 January. The payment landed in January and February still stood as
    /// planned, so a single bill was counted twice -- once as what was paid and again as what was
    /// expected -- and the projection lost about twenty thousand pounds it never spent.
    ///
    /// A payment settles the occurrence nearest to it. Which occurrence a payment clears is
    /// arithmetic on dates, not a guess: the author has already said the payment belongs to this
    /// item.
    /// </remarks>
    [Fact]
    public void Realise_RecurringPaymentMadeEarly_SettlesThatOccurrence()
    {
        var januaryPayment = Guid.NewGuid();
        var fees = LinkedTo(Recurring("School Fees", 21_000m, new DateOnly(2026, 2, 4), ScheduleFrequency.Yearly), januaryPayment);

        // Paid on the 29th of January against a February occurrence.
        var realised = Realise(Plan(fees), new DateOnly(2026, 6, 1), Payment(januaryPayment, new DateOnly(2026, 1, 1), 19_206m));

        Assert.Equal(19_206m, Month(realised.ExpensesByMonth, 2026, 1));
        Assert.Equal(0m, Month(realised.ExpensesByMonth, 2026, 2));

        // One bill, not two.
        Assert.Equal(19_206m, realised.ExpensesByMonth.Values.Sum());
    }

    /// <summary>
    /// Given a payment marked as excluded from reporting
    /// When the plan is realised
    /// Then it should still pay off the item, but not be taken back out of the baseline
    /// </summary>
    /// <remarks>
    /// Keeping a large one-off out of the reports is the same instinct as planning for it, so these
    /// are exactly the payments a planned item is most likely to be waiting for -- the solar
    /// installation this was found on was marked that way. The money left the account, so the item
    /// is paid.
    ///
    /// The baseline is the other way round. Both the lookback average and the expense model's
    /// training data come from procedures that skip these transactions, so this spending was never
    /// in them; subtracting it would take out something that was never there.
    /// </remarks>
    [Fact]
    public void Realise_PaymentExcludedFromReporting_PaysTheItemButIsNotTakenOutOfTheBaseline()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Solar", 17_238.40m, new DateOnly(2026, 3, 6)), id));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Payment(id, new DateOnly(2026, 3, 1), 17_238.40m, inReporting: false));

        Assert.Equal(17_238.40m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(17_238.40m, Assert.Single(realised.Progress).ActualToDate);
        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
    }

    /// <summary>
    /// Given a payment made from an account outside the historical-analysis set
    /// When the plan is realised
    /// Then it should pay the item but not be taken out of the baseline
    /// </summary>
    /// <remarks>
    /// A car paid for out of savings is still the car, so it pays the item. But the baseline it
    /// would be taken from was never computed over savings accounts.
    /// </remarks>
    [Fact]
    public void Realise_PaidFromASavingsAccount_PaysTheItemButNotTheBaseline()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("New Car", 50_000m, new DateOnly(2026, 3, 15)), id));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Payment(id, new DateOnly(2026, 3, 1), 50_000m, account: SavingsId));

        Assert.Equal(50_000m, Month(realised.ExpensesByMonth, 2026, 3));
        Assert.Equal(50_000m, Assert.Single(realised.Progress).ActualToDate);
        Assert.DoesNotContain(realised.AttributedByMonth, m => m.Value != 0m);
    }

    /// <summary>
    /// Given a payment reporting counts, on an account the baseline covers
    /// When the plan is realised
    /// Then it should be taken back out of the baseline
    /// </summary>
    [Fact]
    public void Realise_PaymentTheBaselineContained_IsTakenBackOutOfIt()
    {
        var id = Guid.NewGuid();
        var plan = Plan(LinkedTo(OneOff("Solar", 17_238.40m, new DateOnly(2026, 3, 6)), id));

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Payment(id, new DateOnly(2026, 3, 1), 17_238.40m));

        Assert.Equal(17_238.40m, Month(realised.AttributedByMonth, 2026, 3));
    }

    /// <summary>
    /// Given two items that would share a category
    /// When only one has the payment linked
    /// Then only that item claims it
    /// </summary>
    /// <remarks>
    /// The case the linking mechanism exists for. Solar and Fence are both Home Improvements, and no
    /// rule over tags and dates can say which a payment belongs to -- so nothing but the link does.
    /// </remarks>
    [Fact]
    public void Realise_OnlyTheLinkedItemClaimsThePayment()
    {
        var id = Guid.NewGuid();
        var solar = LinkedTo(OneOff("Solar", 17_238.40m, new DateOnly(2026, 3, 6)), id);
        var fence = OneOff("Fence", 18_600m, new DateOnly(2026, 3, 20));
        var plan = Plan(solar, fence);

        var realised = Realise(plan, new DateOnly(2026, 4, 1), Payment(id, new DateOnly(2026, 3, 1), 17_238.40m));

        Assert.Equal(17_238.40m, realised.Progress.Single(p => p.PlannedItemId == solar.Id).ActualToDate);
        Assert.Equal(0m, realised.Progress.Single(p => p.PlannedItemId == fence.Id).ActualToDate);
    }

    /// <summary>
    /// Given a planned income item
    /// When the plan is realised
    /// Then it should stand as planned and never be measured
    /// </summary>
    /// <remarks>
    /// Income is the plan's own statement of what will arrive. Nothing is averaged or fitted from it
    /// the way it is from expenses, so measuring it would add a class of error and buy nothing.
    /// </remarks>
    [Fact]
    public void Realise_IncomeItem_IsNeverMeasured()
    {
        var salary = new DomainPlannedItem(Guid.NewGuid())
        {
            Name = "Salary",
            ItemType = PlannedItemType.Income,
            Amount = 5_000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Monthly, AnchorDate = PlanStart, Interval = 1 },
        };

        var realised = Realise(Plan(salary), new DateOnly(2026, 6, 1));

        Assert.Equal(5_000m, Month(realised.IncomeByMonth, 2026, 1));
        Assert.Equal(5_000m, Month(realised.IncomeByMonth, 2026, 12));
        Assert.False(Assert.Single(realised.Progress).IsMatched);
        Assert.DoesNotContain(realised.ExpensesByMonth, m => m.Value != 0m);
    }
}
