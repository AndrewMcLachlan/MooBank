using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Tag;

[PrimaryKey(nameof(PrimaryTagId), nameof(SecondaryTagId))]
public partial class TagTag
{
    public int PrimaryTagId { get; set; }

    public int SecondaryTagId { get; set; }

    [Navigation]
    public virtual partial Tag Primary { get; set; }

    [Navigation]
    public virtual partial Tag Secondary { get; set; }
}
