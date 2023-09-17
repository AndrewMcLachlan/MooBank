using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;
public class TransactionSplit : KeyedEntity<Guid>
{
    public TransactionSplit() : base(Guid.NewGuid())
    {
    }

    public TransactionSplit([DisallowNull] Guid id) : base(id)
    {
    }

    public Guid TransactionId { get; set; }

    public decimal Amount { get; set; }

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
}
