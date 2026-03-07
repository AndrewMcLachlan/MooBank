using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Tag;

[PrimaryKey(nameof(PrimaryTagId), nameof(SecondaryTagId))]
public partial class TagTag
{
    public int PrimaryTagId { get; set; }

    public int SecondaryTagId { get; set; }

    public virtual Tag Primary { get; set; } = null!;

    public virtual Tag Secondary { get; set; } = null!;
}
