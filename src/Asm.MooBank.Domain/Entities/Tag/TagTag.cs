namespace Asm.MooBank.Domain.Entities.Tag;

public partial class TagTag
{
    public int PrimaryTransactionTagId { get; set; }

    public int SecondaryTransactionTagId { get; set; }

    public virtual Tag Primary { get; set; } = null!;

    public virtual Tag Secondary { get; set; } = null!;
}
