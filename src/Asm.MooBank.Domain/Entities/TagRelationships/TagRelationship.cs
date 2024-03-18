using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.TagRelationships;

[Table("TagHierarchies", Schema = "dbo")]
[AggregateRoot]
public class TagRelationship
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public long Ordinal { get; set; }

    [AllowNull]
    public virtual Tag.Tag TransactionTag { get; set; }

    [AllowNull]
    public virtual Tag.Tag ParentTag { get; set; }
}
