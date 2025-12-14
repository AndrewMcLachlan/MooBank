using System.Threading;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions.Events;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public partial class Transaction(Guid id) : KeyedEntity<Guid>(id)
{
    private readonly List<TransactionSplit> _splits = [];
    private readonly List<TransactionOffset> _offsetFor = [];

    public Transaction() : this(default) { }

    #region Factory
    public static Transaction Create(
            TransactionInstrument account,
            Guid? accountHolderId,
            decimal amount,
            string? description,
            DateTime transactionTime,
            TransactionSubType? transactionSubType,
            string source)
    {
        var transactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit;

        Transaction transaction = new()
        {
            Account = account,
            AccountHolderId = accountHolderId,
            Amount = amount,
            Description = description,
            TransactionTime = transactionTime,
            TransactionType = transactionType,
            TransactionSubType = transactionSubType,
            Source = source
        };

        transaction.EnsureMinimumSplit();

        transaction.Events.Add(new TransactionAddedEvent(transaction));

        return transaction;
    }

    public static Transaction Create(
        Guid accountId,
        Guid? accountHolderId,
        decimal amount,
        string? description,
        DateTime transactionTime,
        TransactionSubType? transactionSubType,
        string source,
        Guid? institutionAccountId,
        TransactionType? transactionType = null)
    {
        transactionType ??= amount < 0 ? TransactionType.Debit : TransactionType.Credit;

        Transaction transaction = new()
        {
            AccountId = accountId,
            AccountHolderId = accountHolderId,
            Amount = amount,
            Description = description,
            TransactionTime = transactionTime,
            TransactionType = transactionType.Value,
            TransactionSubType = transactionSubType,
            Source = source,
            InstitutionAccountId = institutionAccountId,
        };

        transaction.EnsureMinimumSplit();

        transaction.Events.Add(new TransactionAddedEvent(transaction));

        return transaction;
    }
    #endregion

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

    public TransactionSubType? TransactionSubType { get; set; }

    public required string Source { get; set; }

    public object? Extra { get; set; }

    public Guid? InstitutionAccountId { get; set; }
    #endregion

    #region Navigation Properties
    [BackingField(nameof(_splits))]
    public IReadOnlyCollection<TransactionSplit> Splits => _splits.AsReadOnly();

    public TransactionInstrument Account { get; set; } = null!;

    [ForeignKey(nameof(AccountHolderId))]
    public User.User? User { get; set; }

    [BackingField(nameof(_offsetFor))]
    public IReadOnlyCollection<TransactionOffset> OffsetFor => _offsetFor.AsReadOnly();
    #endregion

    #region Properties
    [NotMapped]
    public IEnumerable<Tag.Tag> Tags => Splits.SelectMany(ts => ts.Tags);

    [NotMapped]
    public decimal NetAmount
    {
        get
        {
            decimal sum = Splits.Sum(s => s.GetNetAmount());
            sum -= OffsetFor.Sum(o => o.Amount);

            if (TransactionType == TransactionType.Debit)
            {
                sum = -sum;
            }

            return sum;
        }
    }
    #endregion

    #region Methods
    public void UpdateSplits(IEnumerable<Models.TransactionSplit> updatedSplits)
    {
        var currentSplitIds = _splits.Select(s => s.Id).ToHashSet();
        var updatedSplitIds = updatedSplits.Select(s => s.Id).ToHashSet();

        // Remove splits that are no longer present
        var splitsToRemove = _splits.Where(s => !updatedSplitIds.Contains(s.Id)).ToList();
        foreach (var split in splitsToRemove)
        {
            RemoveSplit(split);
        }

        // Add new splits
        var splitsToAdd = updatedSplits.Where(s => !currentSplitIds.Contains(s.Id));
        foreach (var splitModel in splitsToAdd)
        {
            AddSplit(splitModel);
        }

        // Update existing splits
        var splitsToUpdate = updatedSplits.Where(s => currentSplitIds.Contains(s.Id));
        foreach (var splitModel in splitsToUpdate)
        {
            UpdateSplit(splitModel);
        }

        EnsureMinimumSplit();
    }

    public void UpdateProperties(string? notes, bool excludeFromReporting)
    {
        Notes = notes;
        ExcludeFromReporting = excludeFromReporting;
    }

    public void AddOrUpdateSplit(Tag.Tag tag) => AddOrUpdateSplit([tag]);

    public void AddOrUpdateSplit(IEnumerable<Tag.Tag> tags)
    {
        var split = Splits.FirstOrDefault();

        if (split == null)
        {
            split = new()
            {
                Id = 1,
                Tags = [.. tags],
                TransactionId = Id,
                Amount = Math.Abs(Amount),
            };
            _splits.Add(split);
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
                    _splits.Remove(split);
                }

                break;
            }
        }
    }

    private void RemoveSplit(TransactionSplit split)
    {
        if (_splits.Count > 1)
        {
            _splits.Remove(split);
        }
        else
        {
            // Clear the split instead of removing if it's the last one
            split.UpdateTags([]);
            split.OffsetBy.Clear();
        }
    }

    private void AddSplit(Models.TransactionSplit splitModel)
    {
        int nextId = _splits.Count > 0 ? _splits.Max(s => s.Id) + 1 : 1;
        var newSplit = new TransactionSplit(nextId)
        {
            Amount = splitModel.Amount,
            TransactionId = Id,
            // Convert model tags to domain entities
            Tags = [.. splitModel.Tags.Select(t => new Tag.Tag(t.Id))],
        };

        // Add offsets from the model
        foreach (var offsetModel in splitModel.OffsetBy)
        {
            newSplit.OffsetBy.Add(new TransactionOffset
            {
                Amount = offsetModel.Amount,
                TransactionId = Id,
                TransactionSplitId = nextId,
                OffsetTransactionId = offsetModel.Transaction.Id,
            });
        }

        _splits.Add(newSplit);
    }

    private void UpdateSplit(Models.TransactionSplit splitModel)
    {
        var splitEntity = _splits.Single(s => s.Id == splitModel.Id);
        splitEntity.Amount = splitModel.Amount;

        // Update tags using the model data
        var newTags = splitModel.Tags.Select(t => new Tag.Tag(t.Id));
        splitEntity.UpdateTags(newTags);

        // Update offsets
        UpdateSplitOffsets(splitEntity, splitModel.OffsetBy);
    }

    private static void UpdateSplitOffsets(TransactionSplit split, IEnumerable<Models.TransactionOffsetBy> offsetModels)
    {
        var currentOffsetIds = split.OffsetBy.Select(o => o.OffsetTransactionId).ToHashSet();
        var updatedOffsetIds = offsetModels.Select(o => o.Transaction.Id).ToHashSet();

        // Remove offsets that are no longer present
        var offsetsToRemove = split.OffsetBy.Where(o => !updatedOffsetIds.Contains(o.OffsetTransactionId)).ToList();
        foreach (var offset in offsetsToRemove)
        {
            split.OffsetBy.Remove(offset);
        }

        // Add new offsets
        var offsetsToAdd = offsetModels.Where(o => !currentOffsetIds.Contains(o.Transaction.Id));
        foreach (var offsetModel in offsetsToAdd)
        {
            split.OffsetBy.Add(new TransactionOffset
            {
                Amount = offsetModel.Amount,
                TransactionId = Id,
                TransactionSplitId = split.Id,
                OffsetTransactionId = offsetModel.Transaction.Id,
            });
        }

        // Update existing offsets
        var offsetsToUpdate = offsetModels.Where(o => currentOffsetIds.Contains(o.Transaction.Id));
        foreach (var offsetModel in offsetsToUpdate)
        {
            var offset = split.OffsetBy.First(o => o.OffsetTransactionId == offsetModel.Transaction.Id);
            offset.Amount = offsetModel.Amount;
        }
    }

    internal void EnsureMinimumSplit()
    {
        if (_splits.Count == 0)
        {
            _splits.Add(new TransactionSplit
            {
                Id = 1,
                Amount = Math.Abs(Amount),
                TransactionId = Id
            });
        }
    }

    /// <summary>
    /// Database function to calculate the net amount of a transaction.
    /// </summary>
    /// <param name="transactionType">Transaction type.</param>
    /// <param name="transactionId">Transaction ID.</param>
    /// <param name="amount">Transaction amount.</param>
    /// <returns>The net amount of the transaction.</returns>
    /// <exception cref="NotSupportedException">Thrown if called outside of a database context operation.</exception>
    [DbFunction("TransactionNetAmount", "dbo")]
    public static decimal TransactionNetAmount(TransactionType transactionType, Guid? transactionId, decimal amount) => throw new NotSupportedException();
    #endregion
}
