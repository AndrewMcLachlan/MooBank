using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement;

/// <summary>
/// A saved set of retirement assumptions for a family.
/// </summary>
/// <remarks>
/// The plan stores assumptions and which superannuation accounts belong to whom; it deliberately
/// does not store balances or projected figures. Projections are recalculated from the members'
/// live account balances each time they are run, so a plan never goes stale.
/// </remarks>
[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class RetirementPlan(Guid id) : KeyedEntity<Guid>(id)
{
    private readonly List<RetirementPlanMember> _members = [];

    public RetirementPlan() : this(Guid.Empty) { }

    public static RetirementPlan Create(Guid familyId, string name, RetirementAssumptions assumptions)
    {
        var plan = new RetirementPlan(Guid.NewGuid())
        {
            FamilyId = familyId,
            Name = name,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        plan.ApplyAssumptions(assumptions);

        return plan;
    }

    public Guid FamilyId { get; set; }

    [ForeignKey(nameof(FamilyId))]
    public virtual Family.Family Family { get; set; } = null!;

    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// The assumed nominal return on superannuation balances, as a rate (0.065 is 6.5% a year).
    /// </summary>
    [Precision(6, 4)]
    public decimal ExpectedReturnRate { get; set; }

    /// <summary>
    /// The assumed rate of inflation, used to express projections in today's dollars.
    /// </summary>
    [Precision(6, 4)]
    public decimal InflationRate { get; set; }

    /// <summary>
    /// The employer superannuation guarantee rate applied to each member's income.
    /// </summary>
    [Precision(6, 4)]
    public decimal SuperGuaranteeRate { get; set; }

    /// <summary>
    /// The tax withheld on concessional contributions as they enter the fund.
    /// </summary>
    [Precision(6, 4)]
    public decimal ContributionsTaxRate { get; set; }

    /// <summary>
    /// The age each member's savings are assumed to need to last until, used to turn a balance at
    /// retirement into an annual drawdown figure.
    /// </summary>
    public int LifeExpectancy { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public IReadOnlyCollection<RetirementPlanMember> Members { get => _members; internal init => _members = [.. value]; }

    public void Update(string name, RetirementAssumptions assumptions)
    {
        Name = name;
        ApplyAssumptions(assumptions);
        UpdatedUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a person to the plan.
    /// </summary>
    /// <remarks>
    /// The member is constructed without an id on purpose. The key is store-generated, and EF
    /// treats an entity that already carries one as a row that exists: adding it to a loaded plan
    /// would then be written as an UPDATE against a row that was never inserted.
    /// </remarks>
    public RetirementPlanMember AddMember(string name, int currentAge, decimal currentIncome, decimal salarySacrifice, int retirementAge, GrowthStrategy growthStrategy, IEnumerable<Guid> instrumentIds)
    {
        var member = new RetirementPlanMember
        {
            RetirementPlanId = Id,
            Name = name,
            CurrentAge = currentAge,
            CurrentIncome = currentIncome,
            SalarySacrifice = salarySacrifice,
            RetirementAge = retirementAge,
            GrowthStrategy = growthStrategy,
        };

        member.SetAccounts(instrumentIds);

        _members.Add(member);
        UpdatedUtc = DateTime.UtcNow;

        return member;
    }

    /// <summary>
    /// Remove a member from the plan.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the member does not belong to this plan.</exception>
    public void RemoveMember(Guid memberId)
    {
        var member = _members.SingleOrDefault(m => m.Id == memberId) ?? throw new NotFoundException("Member not found");

        _members.Remove(member);
        UpdatedUtc = DateTime.UtcNow;
    }

    private void ApplyAssumptions(RetirementAssumptions assumptions)
    {
        ExpectedReturnRate = assumptions.ExpectedReturnRate;
        InflationRate = assumptions.InflationRate;
        SuperGuaranteeRate = assumptions.SuperGuaranteeRate;
        ContributionsTaxRate = assumptions.ContributionsTaxRate;
        LifeExpectancy = assumptions.LifeExpectancy;
    }
}

/// <summary>
/// The economic assumptions a projection is run under.
/// </summary>
/// <param name="ExpectedReturnRate">Nominal return on the balance, as a rate.</param>
/// <param name="InflationRate">Inflation, as a rate. Also used as wage growth.</param>
/// <param name="SuperGuaranteeRate">Employer contribution rate applied to income.</param>
/// <param name="ContributionsTaxRate">Tax withheld on contributions entering the fund.</param>
/// <param name="LifeExpectancy">The age savings must last until.</param>
public readonly record struct RetirementAssumptions(
    decimal ExpectedReturnRate,
    decimal InflationRate,
    decimal SuperGuaranteeRate,
    decimal ContributionsTaxRate,
    int LifeExpectancy);
