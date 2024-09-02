using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[PrimaryKey(nameof(Id))]
public class TransactionSplit : KeyedEntity<Guid>
{
    public TransactionSplit() : base(Guid.Empty)
    {
    }

    public TransactionSplit([DisallowNull] Guid id) : base(id)
    {
    }

    public Guid TransactionId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    [Precision(12, 4)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal NetAmount { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual ICollection<TransactionOffset> OffsetBy { get; set; } = new HashSet<TransactionOffset>();

    public virtual ICollection<Tag.Tag> Tags { get; set; } = new HashSet<Tag.Tag>();

    public void UpdateTags(IEnumerable<Tag.Tag> tags)
    {
        var tagsToRemove = Tags.Where(t => !tags.Any(rt => rt.Id == t.Id)).ToList();
        var tagsToAdd = tags.Where(rt => !Tags.Any(t => t.Id == rt.Id)).ToList();

        foreach (var tag in tagsToRemove)
        {
            Tags.Remove(tag);
        }

        foreach (var tag in tagsToAdd)
        {
            Tags.Add(tag);
        }
    }

    public void RemoveOffset(TransactionOffset offset)
    {
        OffsetBy.Remove(offset);
    }
}
