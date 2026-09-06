using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement;

/// <summary>
/// One person within a retirement plan, with their own income, retirement age and superannuation
/// accounts. A household plan has a member per individual whose super is being projected.
/// </summary>
/// <remarks>
/// Age is held as a number rather than a date of birth: the projection only needs the number of
/// years to retirement, and a date of birth is personal information the application has no reason
/// to hold. The trade-off is that a saved age does not advance on its own — a plan left untouched
/// for a year projects from the age it was last given.
/// </remarks>
[PrimaryKey(nameof(Id))]
public class RetirementPlanMember(Guid id) : KeyedEntity<Guid>(id)
{
    private readonly List<RetirementPlanMemberAccount> _accounts = [];

    public RetirementPlanMember() : this(Guid.Empty) { }

    public Guid RetirementPlanId { get; set; }

    [ForeignKey(nameof(RetirementPlanId))]
    public virtual RetirementPlan RetirementPlan { get; set; } = null!;

    /// <summary>
    /// The person this member is. A plan projects the superannuation of people in the family, so a
    /// member references a user rather than carrying a name of its own — which also means the
    /// accounts it can hold are exactly the ones that user owns.
    /// </summary>
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User.User User { get; set; } = null!;

    public int CurrentAge { get; set; }

    /// <summary>
    /// Current gross annual income, which drives employer contributions.
    /// </summary>
    [Precision(18, 2)]
    public decimal CurrentIncome { get; set; }

    /// <summary>
    /// Additional concessional contributions made from pre-tax income each year, on top of the
    /// employer's.
    /// </summary>
    [Precision(18, 2)]
    public decimal SalarySacrifice { get; set; }

    public int RetirementAge { get; set; }

    /// <summary>
    /// Administration fees charged by the fund each year.
    /// </summary>
    [Precision(18, 2)]
    public decimal AnnualFees { get; set; }

    /// <summary>
    /// Insurance premiums deducted from the balance each year.
    /// </summary>
    [Precision(18, 2)]
    public decimal InsurancePremium { get; set; }

    [Column("GrowthStrategyId")]
    public GrowthStrategy GrowthStrategy { get; set; }

    /// <summary>
    /// The nominal return this member's balance is assumed to earn, when they are on
    /// <see cref="GrowthStrategy.Custom"/>.
    /// </summary>
    /// <remarks>
    /// Held here rather than looked up, which is the whole difference between Custom and a named
    /// strategy: a named one follows the reference rate and moves when it is corrected, while a
    /// custom one is the figure this person chose and stays where they put it.
    /// </remarks>
    [Precision(6, 4)]
    public decimal? CustomReturnRate { get; set; }

    /// <summary>
    /// The strategy this member moves to once retired, if they move at all.
    /// </summary>
    /// <remarks>
    /// A portfolio that suits twenty years of accumulation rarely suits drawing an income from,
    /// and leaving it unset keeps them where they are rather than guessing at a glide.
    /// </remarks>
    [Column("RetirementGrowthStrategyId")]
    public GrowthStrategy? RetirementGrowthStrategy { get; set; }

    /// <inheritdoc cref="CustomReturnRate"/>
    [Precision(6, 4)]
    public decimal? RetirementCustomReturnRate { get; set; }

    /// <summary>
    /// How this member's salary is assumed to grow each year.
    /// </summary>
    /// <remarks>
    /// Absent follows the plan's inflation, which assumes pay keeps pace with prices. That is an
    /// assumption rather than a fact, and one worth being able to contradict: a salary that holds
    /// flat in nominal terms contributes markedly less over twenty years.
    /// </remarks>
    [Precision(6, 4)]
    public decimal? SalaryGrowthRate { get; set; }

    public IReadOnlyCollection<RetirementPlanMemberAccount> Accounts { get => _accounts; internal init => _accounts = [.. value]; }

    public void Update(RetirementMemberDetails details)
    {
        CurrentAge = details.CurrentAge;
        CurrentIncome = details.CurrentIncome;
        SalarySacrifice = details.SalarySacrifice;
        SalaryGrowthRate = details.SalaryGrowthRate;
        RetirementAge = details.RetirementAge;
        GrowthStrategy = details.GrowthStrategy;
        CustomReturnRate = details.CustomReturnRate;
        RetirementGrowthStrategy = details.RetirementGrowthStrategy;
        RetirementCustomReturnRate = details.RetirementCustomReturnRate;
        AnnualFees = details.AnnualFees;
        InsurancePremium = details.InsurancePremium;
    }

    /// <summary>
    /// Replace the set of instruments belonging to this member.
    /// </summary>
    /// <remarks>
    /// Links are constructed without an id: the key is store-generated, and EF reads an entity that
    /// already carries one as a row that exists, which would be written as an UPDATE rather than an
    /// insert. <see cref="RetirementPlan.AddMember"/> has the same constraint.
    /// </remarks>
    public void SetAccounts(IEnumerable<Guid> instrumentIds)
    {
        _accounts.Clear();

        foreach (var instrumentId in instrumentIds.Distinct())
        {
            _accounts.Add(new RetirementPlanMemberAccount
            {
                RetirementPlanMemberId = Id,
                InstrumentId = instrumentId,
            });
        }
    }
}

/// <summary>
/// Everything about a member that a caller sets, as one value.
/// </summary>
/// <param name="CurrentAge">Their age now, from which every year of the projection is counted.</param>
/// <param name="CurrentIncome">Gross annual income, which drives employer contributions.</param>
/// <param name="SalarySacrifice">Additional concessional contributions from pre-tax income.</param>
/// <param name="SalaryGrowthRate">
/// How the income grows each year. Absent follows the plan's inflation.
/// </param>
/// <param name="RetirementAge">The age they stop contributing and start drawing.</param>
/// <param name="GrowthStrategy">What their balance is invested in while accumulating.</param>
/// <param name="CustomReturnRate">Their own rate, when the strategy is Custom.</param>
/// <param name="RetirementGrowthStrategy">What they move to once retired, if anything.</param>
/// <param name="RetirementCustomReturnRate">Their own rate for that, when it is Custom.</param>
/// <param name="AnnualFees">Administration fees charged by the fund each year.</param>
/// <param name="InsurancePremium">Insurance premiums deducted from the balance each year.</param>
public readonly record struct RetirementMemberDetails(
    int CurrentAge,
    decimal CurrentIncome,
    decimal SalarySacrifice,
    decimal? SalaryGrowthRate,
    int RetirementAge,
    GrowthStrategy GrowthStrategy,
    decimal? CustomReturnRate,
    GrowthStrategy? RetirementGrowthStrategy,
    decimal? RetirementCustomReturnRate,
    decimal AnnualFees,
    decimal InsurancePremium);
