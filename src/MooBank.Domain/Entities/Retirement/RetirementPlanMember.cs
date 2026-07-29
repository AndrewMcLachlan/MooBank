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

    public IReadOnlyCollection<RetirementPlanMemberAccount> Accounts { get => _accounts; internal init => _accounts = [.. value]; }

    public void Update(int currentAge, decimal currentIncome, decimal salarySacrifice, int retirementAge, GrowthStrategy growthStrategy, decimal annualFees, decimal insurancePremium)
    {
        CurrentAge = currentAge;
        CurrentIncome = currentIncome;
        SalarySacrifice = salarySacrifice;
        RetirementAge = retirementAge;
        GrowthStrategy = growthStrategy;
        AnnualFees = annualFees;
        InsurancePremium = insurancePremium;
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
