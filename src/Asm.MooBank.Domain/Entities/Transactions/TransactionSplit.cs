using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[PrimaryKey(nameof(TransactionId), nameof(Id))]
public class TransactionSplit : Entity
{
    public TransactionSplit()
    {
    }

    public TransactionSplit(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public Guid TransactionId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual ICollection<TransactionOffset> OffsetBy { get; set; } = [];

    public virtual ICollection<Tag.Tag> Tags { get; set; } = [];

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

    /// <summary>
    /// Mirrors the DB function.
    /// </summary>
    /// <returns></returns>
    public decimal GetNetAmount() => Amount - OffsetBy.Sum(o => o.Amount);

    public static decimal TransactionSplitNetAmount(Guid transactionId, int transactionSplitId, decimal amount) => throw new NotSupportedException();
}
