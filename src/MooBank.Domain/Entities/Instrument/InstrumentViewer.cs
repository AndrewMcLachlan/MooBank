using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[PrimaryKey(nameof(InstrumentId), nameof(UserId))]
public partial class InstrumentViewer
{
    public Guid InstrumentId { get; set; }

    public Guid UserId { get; set; }

    public Guid? GroupId { get; set; }

    [Navigation]
    public virtual partial Instrument Instrument { get; set; }

    [ForeignKey(nameof(UserId))]
    [Navigation]
    public virtual partial User.User User { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual Group.Group? Group { get; set; }
}
