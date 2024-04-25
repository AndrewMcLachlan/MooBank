namespace Asm.MooBank.Domain.Entities.Account;

public partial class InstrumentViewer
{
    public Guid InstrumentId { get; set; }
    public Guid UserId { get; set; }

    public Guid? GroupId { get; set; }

    public virtual Instrument Instrument { get; set; } = null!;

    public virtual AccountHolder.User User { get; set; } = null!;

    public virtual Group.Group Group { get; set; } = null!;
}
