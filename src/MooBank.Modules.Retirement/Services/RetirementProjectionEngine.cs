using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// Projects superannuation balances from today through retirement to life expectancy, one year at a
/// time — accumulating while members work, then drawing down once they have all retired.
/// </summary>
/// <remarks>
/// The model follows the shape of a standard superannuation calculator, and every assumption it
/// makes is stated here rather than buried in the arithmetic:
///
/// <list type="bullet">
/// <item>Contributions are the employer's, at the plan's superannuation guarantee rate, plus any
/// salary sacrifice. Both are concessional, so both are reduced by the contributions tax rate as
/// they enter the fund. After-tax contributions and the concessional cap are not modelled.</item>
/// <item>Income and salary sacrifice grow at the inflation rate, so both hold their real
/// value.</item>
/// <item>A year's investment return is applied to the opening balance; that year's contributions
/// earn nothing until the following year. This is the conservative end of the range — real funds
/// receive contributions through the year.</item>
/// <item>A member stops contributing at their retirement age, and their balance moves to cash a
/// set number of years before that — the de-risking glide most funds apply, on the reasoning that
/// a balance about to be drawn on has no working years left to recover a fall from. It stays in
/// cash through retirement.</item>
/// <item>Drawdown begins the year after every member has retired, not when the first one does: a
/// household with someone still earning lives on that income. From then on the plan's target
/// income is withdrawn each year, indexed to inflation so it holds its real value, and split
/// across the members in proportion to their balances so they deplete together.</item>
/// <item>The Age Pension is worked out first and superannuation covers only the rest of the target.
/// Because the pension is means-tested on what the household holds, it grows as the balances
/// deplete — so a household that spends its superannuation falls back to the pension rather than to
/// nothing. See <see cref="AgePension"/> for what is and is not modelled in that test.</item>
/// <item>A balance cannot go negative. Once the household's balances are exhausted the projection
/// keeps running to life expectancy on the pension alone — and a plan "runs out" when its total
/// income falls short of the target, not when the superannuation is spent.</item>
/// <item>Investment earnings tax within the fund, and tax on withdrawals, are not modelled.</item>
/// </list>
///
/// Projections are indicative arithmetic on the stated assumptions, not a prediction, and not advice.
/// </remarks>
internal class RetirementProjectionEngine : IRetirementProjectionEngine
{
    /// <summary>
    /// Assumed long-run nominal returns for the named investment options.
    /// </summary>
    /// <remarks>
    /// Illustrative figures in the range Australian funds publish for options of each risk level,
    /// not a quote from any particular fund. A member set to <see cref="GrowthStrategy.Custom"/>
    /// uses the rate on the plan instead.
    /// </remarks>
    private static readonly Dictionary<GrowthStrategy, decimal> StrategyReturns = new()
    {
        [GrowthStrategy.Conservative] = 0.045m,
        [GrowthStrategy.Balanced] = 0.060m,
        [GrowthStrategy.Growth] = 0.070m,
        [GrowthStrategy.HighGrowth] = 0.080m,
    };

    /// <summary>
    /// The assumed nominal return for a strategy, falling back to the plan's own rate.
    /// </summary>
    internal static decimal ReturnRateFor(GrowthStrategy strategy, decimal planRate) =>
        StrategyReturns.TryGetValue(strategy, out var rate) ? rate : planRate;

    public RetirementProjection Calculate(DomainEntities.RetirementPlan plan, DateOnly today, AgePensionRates pensionRates, ProjectionOverrides? overrides = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var projection = Project(plan, today, pensionRates, overrides);

        // Nobody to project is nobody to pay: with no members there is no drawdown, so no target is
        // ever unaffordable and the search would run to its own ceiling.
        var sustainable = projection.Members.Any()
            ? SolveSustainableIncome(plan, today, pensionRates, overrides)
            : 0m;

        return projection with
        {
            Summary = projection.Summary with { SustainableIncomeInTodaysDollars = sustainable },
        };
    }

    /// <summary>
    /// The largest target income the plan can pay every year without falling short before its life
    /// expectancy, in today's dollars.
    /// </summary>
    /// <remarks>
    /// Found by running the projection itself and halving the interval, rather than by an annuity on
    /// the closing balance. The two are not close: an annuity knows nothing of the fees and premiums
    /// still being charged, nor of the Age Pension arriving to share the load, and on a real plan it
    /// overstated what was affordable by around a tenth with the pension and a third without.
    ///
    /// Twenty-eight halvings of a range up to a million settle to well under a dollar, and the answer
    /// is rounded to the nearest hundred because a target income is a decision, not a measurement.
    /// Each pass is a few dozen years of arithmetic over a handful of people, so the whole solve costs
    /// far less than the request that carried it.
    /// </remarks>
    private decimal SolveSustainableIncome(DomainEntities.RetirementPlan plan, DateOnly today, AgePensionRates pensionRates, ProjectionOverrides? overrides)
    {
        decimal affordable = 0m, tooMuch = 1_000_000m;

        for (var i = 0; i < 28; i++)
        {
            var candidate = (affordable + tooMuch) / 2m;
            var trial = overrides is null
                ? new ProjectionOverrides { TargetRetirementIncome = candidate }
                : overrides with { TargetRetirementIncome = candidate };

            if (Project(plan, today, pensionRates, trial).Summary.MoneyRunsOutYear is null) affordable = candidate;
            else tooMuch = candidate;
        }

        return Math.Floor(affordable / 100m) * 100m;
    }

    private RetirementProjection Project(DomainEntities.RetirementPlan plan, DateOnly today, AgePensionRates pensionRates, ProjectionOverrides? overrides)
    {

        var assumptions = ResolvedAssumptions.From(plan, overrides);
        var startYear = today.Year;

        var excluded = overrides?.ExcludedMemberIds?.ToHashSet() ?? [];

        var included = plan.Members.Where(m => !excluded.Contains(m.Id)).ToList();

        // Excluding everybody would project nothing at all, which answers no question; treat it as
        // excluding nobody.
        if (included.Count == 0) included = plan.Members.ToList();

        var members = included
            .Select(m => ResolvedMember.From(m, overrides, CurrentBalance(m)))
            .Select(m => new MemberState(m, assumptions))
            .ToList();

        if (members.Count == 0)
        {
            return EmptyProjection(plan, assumptions, startYear);
        }

        var yearsToAllRetired = members.Max(m => m.YearsToRetirement);

        // The projection now runs past retirement to life expectancy, so its horizon is however long
        // the longest-lived member has left rather than however long until the last one retires.
        var horizon = Math.Max(yearsToAllRetired, members.Max(m => m.YearsToLifeExpectancy));

        var years = new List<RetirementProjectionYear>(horizon + 1);

        // Year zero is the starting position: no contributions or returns have been applied yet.
        var openingTotal = members.Sum(m => m.Balance);
        years.Add(new RetirementProjectionYear
        {
            Year = startYear,
            OpeningBalance = openingTotal,
            Contributions = 0m,
            InvestmentReturn = 0m,
            ClosingBalance = openingTotal,
            ClosingBalanceInTodaysDollars = openingTotal,
            AllRetired = yearsToAllRetired == 0,
            Members = members.Select(m => m.ToYear(0, m.Balance, 0m, 0m, 0m, 0m, 0m)).ToList(),
        });

        // The factor that converts a nominal amount in the current year back to today's dollars.
        var todaysDollarsFactor = 1m;

        for (var yearOffset = 1; yearOffset <= horizon; yearOffset++)
        {
            // A year further from today, so a dollar in it is worth a year's inflation less.
            todaysDollarsFactor /= 1m + assumptions.InflationRate;

            var opening = 0m;
            var contributions = 0m;
            var investmentReturn = 0m;
            var costs = 0m;
            var drawdown = 0m;

            var allRetired = members.All(m => yearOffset >= m.YearsToRetirement);

            // Drawing starts the year after the last member retires, not in the retirement year
            // itself — that year they are still working, and still being paid.
            var isDrawingDown = yearOffset > yearsToAllRetired;

            // Split the household's target across the members by balance, worked out before any of
            // them are touched so the shares are consistent with each other.
            var openingTotalThisYear = members.Sum(m => m.Balance);

            var drawdownIndexation = DrawdownIndexation(yearOffset, assumptions.InflationRate);

            // The pension is means-tested on what the household holds, so it is worked out from the
            // opening balances — and it rises as those balances deplete, which is what lets a plan
            // keep paying an income after the superannuation is spent.
            var indexedRates = AgePension.Indexed(pensionRates, drawdownIndexation);
            var ages = members.Select(m => m.AgeAt(yearOffset)).ToArray();

            var pension = isDrawingDown
                ? Round(AgePension.ForYear(indexedRates, ages, openingTotalThisYear))
                : 0m;

            // Reported for every year old enough to qualify, drawdown or not: the balance crossing it
            // is what makes the pension's arrival legible on the curve.
            var pensionCutOff = Round(AgePension.AssetsCutOff(indexedRates, ages));

            // Superannuation covers whatever the pension does not. Drawing the full target on top of
            // the pension would spend the balance faster than the household actually needs to.
            var targetThisYear = isDrawingDown
                ? Math.Max(0m, (assumptions.TargetRetirementIncome * drawdownIndexation) - pension)
                : 0m;

            var memberYears = new List<RetirementMemberYear>(members.Count);

            foreach (var member in members)
            {
                var memberOpening = member.Balance;
                opening += memberOpening;

                var memberReturn = Round(memberOpening * member.ReturnRateInYear(yearOffset, assumptions));
                var memberContribution = member.IsAccumulating(yearOffset)
                    ? Round(member.ContributionForYear(yearOffset, assumptions))
                    : 0m;

                // Taken out year by year rather than as a lump at the end, so the fees paid
                // early lose their compounding too — which is most of what fees actually cost.
                var memberCosts = Round(member.CostsForYear(yearOffset, assumptions));

                // A balance cannot be charged into the red.
                memberCosts = Math.Min(memberCosts, memberOpening + memberReturn + memberContribution);

                var available = memberOpening + memberReturn + memberContribution - memberCosts;

                // Their share of the target, and never more than they have: a member whose balance
                // is exhausted simply stops contributing to the household's income.
                var share = openingTotalThisYear > 0m ? memberOpening / openingTotalThisYear : 0m;
                var memberDrawdown = Math.Min(Round(targetThisYear * share), Math.Max(0m, available));

                member.Balance = available - memberDrawdown;

                investmentReturn += memberReturn;
                contributions += memberContribution;
                costs += memberCosts;
                drawdown += memberDrawdown;

                memberYears.Add(member.ToYear(yearOffset, member.Balance, memberContribution, memberReturn, memberCosts, memberDrawdown, Round(memberDrawdown * todaysDollarsFactor)));

                // Capture the balance the moment this member reaches their retirement age.
                if (yearOffset == member.YearsToRetirement)
                {
                    member.BalanceAtRetirement = member.Balance;
                    member.TodaysDollarsFactorAtRetirement = todaysDollarsFactor;
                }
            }

            var closing = opening + contributions + investmentReturn - costs - drawdown;

            years.Add(new RetirementProjectionYear
            {
                Year = startYear + yearOffset,
                OpeningBalance = opening,
                Contributions = contributions,
                InvestmentReturn = investmentReturn,
                Costs = costs,
                Drawdown = drawdown,
                Pension = pension,
                TotalIncome = drawdown + pension,
                ClosingBalance = closing,
                ClosingBalanceInTodaysDollars = Round(closing * todaysDollarsFactor),
                DrawdownInTodaysDollars = Round(drawdown * todaysDollarsFactor),
                TotalIncomeInTodaysDollars = Round((drawdown + pension) * todaysDollarsFactor),
                PensionInTodaysDollars = Round(pension * todaysDollarsFactor),
                PensionAssetsCutOff = pensionCutOff,
                PensionAssetsCutOffInTodaysDollars = Round(pensionCutOff * todaysDollarsFactor),
                AllRetired = allRetired,
                Members = memberYears,
            });
        }

        var outcomes = members.Select(m => m.ToOutcome(assumptions, startYear)).ToList();

        // The retirement year is where the accumulation phase ends, which is no longer the end of
        // the projection now that drawdown runs on past it.
        var retirementYear = years[yearsToAllRetired];
        var retirementAge = members.Max(m => m.AgeAt(m.YearsToRetirement));
        var finalYear = years[^1];

        return new RetirementProjection
        {
            PlanId = plan.Id,
            Years = years,
            Members = outcomes,
            Summary = new RetirementProjectionSummary
            {
                CurrentBalance = members.Sum(m => m.StartingBalance),
                BalanceAtRetirement = retirementYear.ClosingBalance,
                BalanceAtRetirementInTodaysDollars = retirementYear.ClosingBalanceInTodaysDollars,
                RetirementYear = startYear + yearsToAllRetired,
                RetirementAge = retirementAge,
                RealReturnRate = RealReturnRate(assumptions.ExpectedReturnRate, assumptions.InflationRate),
                DrawdownRealReturnRate = RealReturnRate(assumptions.CashReturnRate, assumptions.InflationRate),
                TotalCosts = years.Sum(y => y.Costs),
                FinalBalance = finalYear.ClosingBalance,
                FinalBalanceInTodaysDollars = finalYear.ClosingBalanceInTodaysDollars,
                LifeExpectancyYear = startYear + horizon,
                MoneyRunsOutYear = MoneyRunsOutYear(years, assumptions, startYear, yearsToAllRetired),
                TotalPension = years.Sum(y => y.Pension),
            },
        };
    }

    /// <summary>
    /// The first year the household could not reach the income it was targeting, or null if it never
    /// fell short.
    /// </summary>
    /// <remarks>
    /// Answers "does this plan hold up" from what the projection actually paid out rather than from
    /// the closing balance: a year that pays a partial income is already the year it ran short, even
    /// though a little may be left in the fund.
    ///
    /// Measured against total income, pension included. Once the pension is modelled, an exhausted
    /// balance is no longer the end of the household's income — a modest target can be met by the
    /// pension alone, and calling that "run out" because the superannuation is spent would be wrong.
    ///
    /// A plan targeting no income cannot run short, so it never reports a year.
    /// </remarks>
    private static int? MoneyRunsOutYear(List<RetirementProjectionYear> years, ResolvedAssumptions assumptions, int startYear, int yearsToAllRetired)
    {
        if (assumptions.TargetRetirementIncome <= 0m) return null;

        for (var yearOffset = yearsToAllRetired + 1; yearOffset < years.Count; yearOffset++)
        {
            var wanted = assumptions.TargetRetirementIncome * DrawdownIndexation(yearOffset, assumptions.InflationRate);

            // A cent of rounding either way is not a shortfall.
            if (years[yearOffset].TotalIncome < wanted - 0.01m)
            {
                return startYear + yearOffset;
            }
        }

        return null;
    }

    /// <summary>
    /// The return above inflation implied by a nominal return and an inflation rate.
    /// </summary>
    /// <remarks>
    /// Uses the Fisher relation rather than subtracting the two rates, which overstates the real
    /// return by the product of the two.
    /// </remarks>
    internal static decimal RealReturnRate(decimal nominalRate, decimal inflationRate) =>
        ((1m + nominalRate) / (1m + inflationRate)) - 1m;


    /// <summary>
    /// A member's combined balance across their selected instruments.
    /// </summary>
    /// <remarks>
    /// Only transaction instruments carry a balance; anything else selected contributes nothing
    /// rather than failing the projection.
    /// </remarks>
    private static decimal CurrentBalance(DomainEntities.RetirementPlanMember member) =>
        member.Accounts
              .Select(a => a.Instrument)
              .OfType<TransactionInstrument>()
              .Sum(i => i.Balance);

    private static RetirementProjection EmptyProjection(DomainEntities.RetirementPlan plan, ResolvedAssumptions assumptions, int startYear) =>
        new()
        {
            PlanId = plan.Id,
            Years = [],
            Members = [],
            Summary = new RetirementProjectionSummary
            {
                RetirementYear = startYear,
                RealReturnRate = RealReturnRate(assumptions.ExpectedReturnRate, assumptions.InflationRate),
                DrawdownRealReturnRate = RealReturnRate(assumptions.CashReturnRate, assumptions.InflationRate),
            },
        };

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// How far a starting figure has been indexed by the given projection year. The first projected
    /// year uses today's figures unindexed.
    /// </summary>
    private static decimal Indexation(int yearOffset, decimal inflationRate)
    {
        var indexation = 1m;

        for (var i = 1; i < yearOffset; i++)
        {
            indexation *= 1m + inflationRate;
        }

        return indexation;
    }

    /// <summary>
    /// How far the target retirement income has been indexed by the given projection year.
    /// </summary>
    /// <remarks>
    /// A full year ahead of <see cref="Indexation"/>, which is not an inconsistency but the point.
    /// Income and fees are stated as today's figures and deliberately go unindexed in the first
    /// projected year, so their real value drifts. The target income is a promise about buying power:
    /// it has to match the discounting exactly, year for year, or a target set at 90,000 would report
    /// as 87,805 in today's dollars for the whole of retirement.
    /// </remarks>
    private static decimal DrawdownIndexation(int yearOffset, decimal inflationRate) =>
        Indexation(yearOffset + 1, inflationRate);

    /// <summary>
    /// A member's running position through the projection.
    /// </summary>
    private sealed class MemberState
    {
        private readonly ResolvedMember _member;

        public MemberState(ResolvedMember member, ResolvedAssumptions assumptions)
        {
            _member = member;
            StartingBalance = member.Balance;
            Balance = member.Balance;
            BalanceAtRetirement = member.Balance;
            YearsToRetirement = Math.Max(0, member.RetirementAge - member.CurrentAge);
            YearsToLifeExpectancy = Math.Max(0, assumptions.LifeExpectancy - member.CurrentAge);
            ReturnRate = ReturnRateFor(member.GrowthStrategy, assumptions.ExpectedReturnRate);
        }

        public int YearsToRetirement { get; }

        /// <summary>
        /// How many projection years this member has left, which is what the drawdown phase runs
        /// for. Zero for someone already past the plan's life expectancy.
        /// </summary>
        public int YearsToLifeExpectancy { get; }

        /// <summary>
        /// The nominal return this member's balance earns while invested in their chosen strategy,
        /// which they may set independently of the rest of the household.
        /// </summary>
        public decimal ReturnRate { get; }

        /// <summary>
        /// The age this member reaches in the given projection year.
        /// </summary>
        public int AgeAt(int yearOffset) => _member.CurrentAge + yearOffset;

        /// <summary>
        /// Whether the balance has moved to cash by the given year: once the member is within the
        /// plan's switch window of their retirement age, and every year after.
        /// </summary>
        /// <remarks>
        /// A member already inside the window, or already retired, is in cash from the first
        /// projected year. Nought switch years means the move happens at retirement rather than
        /// ahead of it — not that it never happens, since a balance being drawn on is in cash either
        /// way. Set the plan's cash rate to the expected return to model no switch at all.
        /// </remarks>
        public bool IsInCash(int yearOffset, ResolvedAssumptions assumptions) =>
            YearsToRetirement - yearOffset <= assumptions.PreRetirementSwitchYears;

        /// <summary>
        /// The nominal return earned in the given year: their strategy's while it is still invested,
        /// the plan's cash rate once the balance has been moved across.
        /// </summary>
        public decimal ReturnRateInYear(int yearOffset, ResolvedAssumptions assumptions) =>
            IsInCash(yearOffset, assumptions) ? assumptions.CashReturnRate : ReturnRate;

        /// <summary>
        /// This member's position at the end of a projection year.
        /// </summary>
        public RetirementMemberYear ToYear(int yearOffset, decimal closing, decimal contributions, decimal investmentReturn, decimal costs, decimal drawdown, decimal drawdownInTodaysDollars) =>
            new()
            {
                MemberId = _member.Id,
                Name = _member.Name,
                Age = AgeAt(yearOffset),
                Contributions = contributions,
                InvestmentReturn = investmentReturn,
                Costs = costs,
                Drawdown = drawdown,
                DrawdownInTodaysDollars = drawdownInTodaysDollars,
                ClosingBalance = closing,
            };

        public decimal StartingBalance { get; }

        public decimal Balance { get; set; }

        public decimal BalanceAtRetirement { get; set; }

        /// <summary>
        /// Converts this member's balance at retirement back to today's dollars. Starts at 1 so a
        /// member who is already retired needs no discounting.
        /// </summary>
        public decimal TodaysDollarsFactorAtRetirement { get; set; } = 1m;

        public bool AlreadyRetired => _member.CurrentAge >= _member.RetirementAge;

        /// <summary>
        /// Whether the member is still contributing in the given projection year.
        /// </summary>
        public bool IsAccumulating(int yearOffset) => yearOffset <= YearsToRetirement;

        /// <summary>
        /// Employer contributions plus salary sacrifice for a projection year, net of contributions
        /// tax. Both grow with inflation, so the first projected year uses today's figures
        /// unindexed.
        /// </summary>
        public decimal ContributionForYear(int yearOffset, ResolvedAssumptions assumptions)
        {
            var indexation = Indexation(yearOffset, assumptions.InflationRate);

            var employer = _member.CurrentIncome * indexation * assumptions.SuperGuaranteeRate;
            var sacrificed = _member.SalarySacrifice * indexation;

            return (employer + sacrificed) * (1m - assumptions.ContributionsTaxRate);
        }

        /// <summary>
        /// Administration fees and insurance premiums for a projection year.
        /// </summary>
        /// <remarks>
        /// Indexed with inflation, like income and salary sacrifice, so they hold their real
        /// value rather than shrinking away over a long projection.
        ///
        /// Both keep being charged after the member reaches their retirement age, for as long as
        /// the projection runs. Fees genuinely do continue; insurance cover usually ceases, so
        /// this is the conservative reading rather than the exact one.
        /// </remarks>
        public decimal CostsForYear(int yearOffset, ResolvedAssumptions assumptions) =>
            (_member.AnnualFees + _member.InsurancePremium) * Indexation(yearOffset, assumptions.InflationRate);

        public RetirementMemberOutcome ToOutcome(ResolvedAssumptions assumptions, int startYear)
        {
            var balanceAtRetirementReal = Round(BalanceAtRetirement * TodaysDollarsFactorAtRetirement);


            return new RetirementMemberOutcome
            {
                MemberId = _member.Id,
                Name = _member.Name,
                CurrentAge = _member.CurrentAge,
                RetirementAge = _member.RetirementAge,
                YearsToRetirement = YearsToRetirement,
                RetirementYear = startYear + YearsToRetirement,
                CurrentBalance = StartingBalance,
                BalanceAtRetirement = BalanceAtRetirement,
                BalanceAtRetirementInTodaysDollars = balanceAtRetirementReal,
                AlreadyRetired = AlreadyRetired,
                GrowthStrategy = _member.GrowthStrategy,
                ReturnRate = ReturnRate,
            };
        }
    }
}
