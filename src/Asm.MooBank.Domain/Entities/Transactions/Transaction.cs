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

    #region Columns
    public Guid TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public Guid? AccountHolderId { get; set; }

    public decimal Amount { get; set; }

    public decimal NetAmount { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? Reference { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime TransactionTime { get; set; }

    public string? Notes { get; set; }

    public bool ExcludeFromReporting { get; set; }

    public TransactionType TransactionType { get; set; }

    public string? Source { get; set; }

    public object? Extra { get; set; }
    #endregion

    #region Navigation Properties
    public virtual ICollection<TransactionSplit> Splits { get; set; } = new HashSet<TransactionSplit>();

    public virtual Account.Account Account { get; set; } = null!;

    public virtual AccountHolder.AccountHolder? AccountHolder { get; set; }

    public virtual ICollection<TransactionOffset> OffsetFor { get; set; } = new HashSet<TransactionOffset>();
    #endregion

    #region Properties
    [NotMapped]
    public IEnumerable<Tag.Tag> Tags => Splits.SelectMany(ts => ts.Tags);
    #endregion

    #region Methods
    public void AddOrUpdateSplit(Tag.Tag tag) => AddOrUpdateSplit(new[] { tag });

    public void AddOrUpdateSplit(IEnumerable<Tag.Tag> tags)
    {
        var split = Splits.FirstOrDefault();

        if (split == null)
        {
            split = new()
            {
                Tags = tags.ToList(),
                TransactionId = TransactionId,
                Amount = Amount,
            };
            Splits.Add(split);
            return;
        }

        split.Tags.AddRange(tags.Except(split.Tags, new TagEqualityComparer()));
    }

    public void UpdateOrRemoveSplit(Tag.Tag tag)
    {
        foreach (var split in Splits)
        {
            if (split.Tags.Contains(tag, new TagEqualityComparer()))
            {
                split.Tags.Remove(tag);

                if (split.Tags.Count == 0)
                {
                    Splits.Remove(split);
                }

                break;
            }
        }
    }

    public void RemoveSplit(TransactionSplit split)
    {
        Splits.Remove(split);
    }
    #endregion
}
