using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.TransactionTagHierarchies;

[Table("TagHierarchies",Schema = "dbo")]
[AggregateRoot]
public class TransactionTagRelationship
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public long Ordinal { get; set; }

    public virtual Tag.Tag TransactionTag { get; set; }

    public virtual Tag.Tag ParentTag { get; set; }
}
