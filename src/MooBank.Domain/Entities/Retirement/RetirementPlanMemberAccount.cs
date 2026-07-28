using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement;

/// <summary>
/// Links a superannuation instrument to the plan member it belongs to.
/// </summary>
[PrimaryKey(nameof(Id))]
public class RetirementPlanMemberAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public RetirementPlanMemberAccount() : this(Guid.Empty) { }

    public Guid RetirementPlanMemberId { get; set; }

    [ForeignKey(nameof(RetirementPlanMemberId))]
    public virtual RetirementPlanMember RetirementPlanMember { get; set; } = null!;

    public Guid InstrumentId { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    public virtual Instrument.Instrument Instrument { get; set; } = null!;
}
