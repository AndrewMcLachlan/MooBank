using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[PrimaryKey(nameof(InstrumentId), nameof(UserId))]
public partial class InstrumentViewer
{
    public Guid InstrumentId { get; set; }

    public Guid UserId { get; set; }

    public Guid? GroupId { get; set; }

    public virtual Instrument Instrument { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User.User User { get; set; } = null!;

    [ForeignKey(nameof(GroupId))]
    public virtual Group.Group? Group { get; set; }
}
