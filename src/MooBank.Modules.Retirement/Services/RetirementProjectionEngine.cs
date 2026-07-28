using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// Projects superannuation balances forward to retirement, one year at a time.
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
/// <item>A member who has reached their retirement age stops contributing but their balance keeps
/// earning returns. Drawdown is not modelled during the projection, so the household total in a
/// year where one member has retired and another has not assumes the retired member has not
/// started spending.</item>
/// <item>Fees, insurance premiums, investment earnings tax within the fund, the Age Pension and
/// any tax on withdrawals are not modelled.</item>
/// </list>
///
/// Because of the last point in particular, projections are indicative arithmetic on the stated
/// assumptions and not a prediction or advice.
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

    public RetirementProjection Calculate(DomainEntities.RetirementPlan plan, DateOnly today, ProjectionOverrides? overrides = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var assumptions = ResolvedAssumptions.From(plan, overrides);
        var startYear = today.Year;

        var members = plan.Members
            .Select(m => ResolvedMember.From(m, overrides, CurrentBalance(m)))
            .Select(m => new MemberState(m, assumptions))
            .ToList();

        if (members.Count == 0)
        {
            return EmptyProjection(plan, assumptions, startYear);
        }

        var horizon = members.Max(m => m.YearsToRetirement);

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
            AllRetired = horizon == 0,
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

            foreach (var member in members)
            {
                opening += member.Balance;

                var memberReturn = Round(member.Balance * member.ReturnRate);
                var memberContribution = member.IsAccumulating(yearOffset)
                    ? Round(member.ContributionForYear(yearOffset, assumptions))
                    : 0m;

                member.Balance += memberReturn + memberContribution;

                investmentReturn += memberReturn;
                contributions += memberContribution;

                // Capture the balance the moment this member reaches their retirement age.
                if (yearOffset == member.YearsToRetirement)
                {
                    member.BalanceAtRetirement = member.Balance;
                    member.TodaysDollarsFactorAtRetirement = todaysDollarsFactor;
                }
            }

            var closing = opening + contributions + investmentReturn;

            years.Add(new RetirementProjectionYear
            {
                Year = startYear + yearOffset,
                OpeningBalance = opening,
                Contributions = contributions,
                InvestmentReturn = investmentReturn,
                ClosingBalance = closing,
                ClosingBalanceInTodaysDollars = Round(closing * todaysDollarsFactor),
                AllRetired = members.All(m => yearOffset >= m.YearsToRetirement),
            });
        }

        var outcomes = members.Select(m => m.ToOutcome(assumptions, startYear)).ToList();

        var finalYear = years[^1];

        return new RetirementProjection
        {
            PlanId = plan.Id,
            Years = years,
            Members = outcomes,
            Summary = new RetirementProjectionSummary
            {
                CurrentBalance = members.Sum(m => m.StartingBalance),
                BalanceAtRetirement = finalYear.ClosingBalance,
                BalanceAtRetirementInTodaysDollars = finalYear.ClosingBalanceInTodaysDollars,
                AnnualRetirementIncomeInTodaysDollars = outcomes.Sum(o => o.AnnualRetirementIncomeInTodaysDollars),
                RetirementYear = startYear + horizon,
                RealReturnRate = RealReturnRate(assumptions.ExpectedReturnRate, assumptions.InflationRate),
            },
        };
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
    /// The level annual payment a balance supports over a number of years, in the same dollars as
    /// the balance.
    /// </summary>
    /// <param name="balance">The balance at the start of the drawdown.</param>
    /// <param name="realReturnRate">The return earned during drawdown, above inflation.</param>
    /// <param name="years">How many years the balance must last.</param>
    /// <remarks>
    /// The standard present-value-of-an-annuity formula, rearranged for the payment. When the real
    /// return is negligible it degenerates to dividing the balance evenly across the years, which
    /// is also the correct answer in the limit.
    /// </remarks>
    internal static decimal AnnualDrawdown(decimal balance, decimal realReturnRate, int years)
    {
        if (years <= 0 || balance <= 0m) return 0m;

        if (Math.Abs(realReturnRate) < 0.000001m) return Round(balance / years);

        var discountFactor = 1d - Math.Pow(1d + (double)realReturnRate, -years);

        // A real return of -100% or worse leaves nothing to draw on and would divide by zero.
        if (discountFactor <= 0d) return 0m;

        return Round(balance * realReturnRate / (decimal)discountFactor);
    }

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
            },
        };

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

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
            ReturnRate = ReturnRateFor(member.GrowthStrategy, assumptions.ExpectedReturnRate);
        }

        public int YearsToRetirement { get; }

        /// <summary>
        /// The nominal return this member's balance earns, which their growth strategy may set
        /// independently of the rest of the household.
        /// </summary>
        public decimal ReturnRate { get; }

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
            var indexation = 1m;

            for (var i = 1; i < yearOffset; i++)
            {
                indexation *= 1m + assumptions.InflationRate;
            }

            var employer = _member.CurrentIncome * indexation * assumptions.SuperGuaranteeRate;
            var sacrificed = _member.SalarySacrifice * indexation;

            return (employer + sacrificed) * (1m - assumptions.ContributionsTaxRate);
        }

        public RetirementMemberOutcome ToOutcome(ResolvedAssumptions assumptions, int startYear)
        {
            var balanceAtRetirementReal = Round(BalanceAtRetirement * TodaysDollarsFactorAtRetirement);
            var drawdownYears = assumptions.LifeExpectancy - _member.RetirementAge;

            // Drawdown earns this member's own return, not the household's.
            var realReturnRate = RealReturnRate(ReturnRate, assumptions.InflationRate);

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
                AnnualRetirementIncomeInTodaysDollars = AnnualDrawdown(balanceAtRetirementReal, realReturnRate, drawdownYears),
                AlreadyRetired = AlreadyRetired,
                GrowthStrategy = _member.GrowthStrategy,
                ReturnRate = ReturnRate,
            };
        }
    }
}
