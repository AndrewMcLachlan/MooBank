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
/// <item>Contributions are employer contributions only, at the plan's superannuation guarantee
/// rate, reduced by the contributions tax rate as they enter the fund. Salary sacrifice,
/// after-tax contributions and the concessional cap are not modelled.</item>
/// <item>Income grows at the inflation rate, so a member's contributions hold their real value.</item>
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
    public RetirementProjection Calculate(DomainEntities.RetirementPlan plan, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var startYear = today.Year;
        var members = plan.Members.Select(m => MemberState.From(m, today)).ToList();

        if (members.Count == 0)
        {
            return EmptyProjection(plan, startYear);
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
            todaysDollarsFactor /= 1m + plan.InflationRate;

            var opening = 0m;
            var contributions = 0m;
            var investmentReturn = 0m;

            foreach (var member in members)
            {
                opening += member.Balance;

                var memberReturn = Round(member.Balance * plan.ExpectedReturnRate);
                var memberContribution = member.IsAccumulating(yearOffset)
                    ? Round(member.IncomeForYear(yearOffset, plan.InflationRate) * plan.SuperGuaranteeRate * (1m - plan.ContributionsTaxRate))
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

        var realReturnRate = RealReturnRate(plan.ExpectedReturnRate, plan.InflationRate);

        var outcomes = members.Select(m => m.ToOutcome(plan, startYear, realReturnRate)).ToList();

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
                RealReturnRate = realReturnRate,
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

    private static RetirementProjection EmptyProjection(DomainEntities.RetirementPlan plan, int startYear) =>
        new()
        {
            PlanId = plan.Id,
            Years = [],
            Members = [],
            Summary = new RetirementProjectionSummary
            {
                RetirementYear = startYear,
                RealReturnRate = RealReturnRate(plan.ExpectedReturnRate, plan.InflationRate),
            },
        };

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// A member's running position through the projection.
    /// </summary>
    private sealed class MemberState
    {
        private MemberState(DomainEntities.RetirementPlanMember member, int currentAge, decimal balance)
        {
            Member = member;
            CurrentAge = currentAge;
            StartingBalance = balance;
            Balance = balance;
            BalanceAtRetirement = balance;
            YearsToRetirement = Math.Max(0, member.RetirementAge - currentAge);
        }

        public DomainEntities.RetirementPlanMember Member { get; }

        public int CurrentAge { get; }

        public int YearsToRetirement { get; }

        public decimal StartingBalance { get; }

        public decimal Balance { get; set; }

        public decimal BalanceAtRetirement { get; set; }

        /// <summary>
        /// Converts this member's balance at retirement back to today's dollars. Starts at 1 so a
        /// member who is already retired needs no discounting.
        /// </summary>
        public decimal TodaysDollarsFactorAtRetirement { get; set; } = 1m;

        public bool AlreadyRetired => CurrentAge >= Member.RetirementAge;

        public static MemberState From(DomainEntities.RetirementPlanMember member, DateOnly today) =>
            new(member, member.AgeAt(today), CurrentBalance(member));

        /// <summary>
        /// Whether the member is still contributing in the given projection year.
        /// </summary>
        public bool IsAccumulating(int yearOffset) => yearOffset <= YearsToRetirement;

        /// <summary>
        /// The member's income in a projection year, grown from today's income at the inflation
        /// rate. The first projected year uses today's income unindexed.
        /// </summary>
        public decimal IncomeForYear(int yearOffset, decimal inflationRate)
        {
            var income = Member.CurrentIncome;

            for (var i = 1; i < yearOffset; i++)
            {
                income *= 1m + inflationRate;
            }

            return income;
        }

        public RetirementMemberOutcome ToOutcome(DomainEntities.RetirementPlan plan, int startYear, decimal realReturnRate)
        {
            var balanceAtRetirementReal = Round(BalanceAtRetirement * TodaysDollarsFactorAtRetirement);
            var drawdownYears = plan.LifeExpectancy - Member.RetirementAge;

            return new RetirementMemberOutcome
            {
                MemberId = Member.Id,
                Name = Member.Name,
                CurrentAge = CurrentAge,
                RetirementAge = Member.RetirementAge,
                YearsToRetirement = YearsToRetirement,
                RetirementYear = startYear + YearsToRetirement,
                CurrentBalance = StartingBalance,
                BalanceAtRetirement = BalanceAtRetirement,
                BalanceAtRetirementInTodaysDollars = balanceAtRetirementReal,
                AnnualRetirementIncomeInTodaysDollars = AnnualDrawdown(balanceAtRetirementReal, realReturnRate, drawdownYears),
                AlreadyRetired = AlreadyRetired,
            };
        }

        /// <summary>
        /// The member's combined balance across their selected instruments.
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
    }
}
