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

    public void SetAccounts(IEnumerable<Guid> instrumentIds)
    {
        _accounts.Clear();

        foreach (var instrumentId in instrumentIds.Distinct())
        {
            _accounts.Add(new RetirementPlanMemberAccount(Guid.NewGuid())
            {
                RetirementPlanMemberId = Id,
                InstrumentId = instrumentId,
            });
        }
    }
}
