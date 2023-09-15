using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
public partial class Transaction
{
    public Transaction()
    {
    }

    public Guid TransactionId { get; set; }

    public Guid? TransactionReference { get; set; }

    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public decimal NetAmount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionTime { get; set; }

    public string? Notes { get; set; }

    public bool ExcludeFromReporting { get; set; }

    public Guid? OffsetByTransactionId { get; set; }

    public virtual ICollection<TransactionSplit> TransactionSplits { get; set; } = new HashSet<TransactionSplit>();

    [NotMapped]
    public IEnumerable<Tag.Tag> Tags => TransactionSplits.SelectMany(ts => ts.Tags);

    public virtual Account.Account Account { get; set; }

    public virtual ICollection<TransactionOffset> OffsetBy { get; set; } = new HashSet<TransactionOffset>();

    public virtual ICollection<TransactionOffset> Offsets { get; set; } = new HashSet<TransactionOffset>();

    public TransactionType TransactionType { get; set; }

    public void AddOrUpdateSplit(Tag.Tag tag) => AddOrUpdateSplit(new[] { tag });

    public void AddOrUpdateSplit(IEnumerable<Tag.Tag> tags)
    {
        var split = TransactionSplits.FirstOrDefault();

        if (split == null)
        {
            split = new()
            {
                Tags = tags.ToList(),
                TransactionId = TransactionId,
                Amount = Amount,
            };
            TransactionSplits.Add(split);
            return;
        }

        split.Tags.AddRange(tags.Except(split.Tags, new TagEqualityComparer()));
    }

    public void UpdateOrRemoveSplit(Tag.Tag tag)
    {
        foreach (var split in TransactionSplits)
        {
            if (split.Tags.Contains(tag, new TagEqualityComparer()))
            {
                split.Tags.Remove(tag);

                if (!split.Tags.Any())
                {
                    TransactionSplits.Remove(split);
                }

                break;
            }
        }
    }

    public void RemoveOffset(TransactionOffset offset)
    {
        OffsetBy.Remove(offset);
    }
}
