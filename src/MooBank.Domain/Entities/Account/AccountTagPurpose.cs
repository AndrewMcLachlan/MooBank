using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(InstrumentId), nameof(Purpose))]
public partial class AccountTagPurpose
{
    public Guid InstrumentId { get; set; }

    public TagPurpose Purpose { get; set; }

    public int TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    [Navigation]
    public virtual partial Tag.Tag Tag { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [Navigation]
    public virtual partial LogicalAccount LogicalAccount { get; set; }
}
