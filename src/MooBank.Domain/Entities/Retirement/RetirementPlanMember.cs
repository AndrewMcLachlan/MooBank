using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement;

/// <summary>
/// One person within a retirement plan, with their own income, retirement age and superannuation
/// accounts. A household plan has a member per individual whose super is being projected.
/// </summary>
[PrimaryKey(nameof(Id))]
public class RetirementPlanMember(Guid id) : KeyedEntity<Guid>(id)
{
    private readonly List<RetirementPlanMemberAccount> _accounts = [];

    public RetirementPlanMember() : this(Guid.Empty) { }

    public Guid RetirementPlanId { get; set; }

    [ForeignKey(nameof(RetirementPlanId))]
    public virtual RetirementPlan RetirementPlan { get; set; } = null!;

    [MaxLength(200)]
    public required string Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Current gross annual income, which drives employer contributions.
    /// </summary>
    [Precision(18, 2)]
    public decimal CurrentIncome { get; set; }

    public int RetirementAge { get; set; }

    public IReadOnlyCollection<RetirementPlanMemberAccount> Accounts { get => _accounts; internal init => _accounts = [.. value]; }

    /// <summary>
    /// The member's age at a given date, in completed years.
    /// </summary>
    public int AgeAt(DateOnly date)
    {
        var age = date.Year - DateOfBirth.Year;

        // Their birthday has not come round yet this year.
        if (DateOfBirth.AddYears(age) > date) age--;

        return age;
    }

    public void Update(string name, DateOnly dateOfBirth, decimal currentIncome, int retirementAge)
    {
        Name = name;
        DateOfBirth = dateOfBirth;
        CurrentIncome = currentIncome;
        RetirementAge = retirementAge;
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
