using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Domain.Entities.TransactionTagHierarchies;

[Table("TagHierarchies",Schema = "dbo")]
[AggregateRoot]
public class TransactionTagRelationship
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public long Ordinal { get; set; }

    public virtual TransactionTag TransactionTag { get; set; }

    public virtual TransactionTag ParentTag { get; set; }
}
