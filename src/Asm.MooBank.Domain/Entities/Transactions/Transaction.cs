using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public partial class Transaction(Guid id) : KeyedEntity<Guid>(id)
{
    public Transaction() : this(default) { }

    #region Columns
    public Guid AccountId { get; set; }

    public Guid? AccountHolderId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    [MaxLength(255)]
    [Unicode(false)]
    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? Reference { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime TransactionTime { get; set; }

    public string? Notes { get; set; }

    public bool ExcludeFromReporting { get; set; }

    public TransactionType TransactionType { get; set; }

    public required string Source { get; set; }

    public object? Extra { get; set; }
    #endregion

    #region Navigation Properties
    public virtual ICollection<TransactionSplit> Splits { get; set; } = [];

    public virtual TransactionInstrument Account { get; set; } = null!;

    [ForeignKey(nameof(AccountHolderId))]
    public virtual User.User? User { get; set; }

    public virtual ICollection<TransactionOffset> OffsetFor { get; set; } = [];
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
                TransactionId = Id,
                Amount = Math.Abs(Amount),
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

                if (split.Tags.Count == 0 && Splits.Count > 1)
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

    public decimal GetNetAmount()
    {
        decimal sum = Splits.Sum(s => s.GetNetAmount());
        sum -= OffsetFor.Sum(o => o.Amount);

        return sum;
    }

    public static decimal TransactionNetAmount(TransactionType transactionType, Guid transactionId, decimal amount) => throw new NotSupportedException();
    #endregion
}
