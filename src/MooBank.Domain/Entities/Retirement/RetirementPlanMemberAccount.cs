using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement;

/// <summary>
/// Links a superannuation instrument to the plan member it belongs to.
/// </summary>
[PrimaryKey(nameof(Id))]
public partial class RetirementPlanMemberAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public RetirementPlanMemberAccount() : this(Guid.Empty) { }

    public Guid RetirementPlanMemberId { get; set; }

    [ForeignKey(nameof(RetirementPlanMemberId))]
    [Navigation]
    public virtual partial RetirementPlanMember RetirementPlanMember { get; set; }

    public Guid InstrumentId { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [Navigation]
    public virtual partial Instrument.Instrument Instrument { get; set; }
}
