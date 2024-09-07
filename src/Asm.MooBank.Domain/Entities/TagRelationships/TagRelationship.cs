using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.TagRelationships;

[Table("TagHierarchies", Schema = "dbo")]
[AggregateRoot]
[PrimaryKey(nameof(Id), nameof(ParentId))]
public class TagRelationship
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public long Ordinal { get; set; }

    [AllowNull]
    [ForeignKey(nameof(Id))]
    public virtual Tag.Tag Tag { get; set; }

    [AllowNull]
    [ForeignKey(nameof(ParentId))]
    public virtual Tag.Tag ParentTag { get; set; }
}
