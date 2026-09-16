using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.TagRelationships;

[Table("TagHierarchies", Schema = "dbo")]
[PrimaryKey(nameof(Id), nameof(ParentId))]
public partial class TagRelationship
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public long Ordinal { get; set; }

    [ForeignKey(nameof(Id))]
    [Navigation]
    public virtual partial Tag.Tag Tag { get; set; }

    [ForeignKey(nameof(ParentId))]
    [Navigation]
    public virtual partial Tag.Tag ParentTag { get; set; }
}
