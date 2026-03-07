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

    #region Database Functions
    private static Func<Guid, Guid, decimal, decimal>? _transactionSplitNetAmountOverride;

    /// <summary>
    /// Sets the override function for <see cref="TransactionSplitNetAmount"/> for unit testing purposes.
    /// </summary>
    internal static void SetTransactionSplitNetAmountOverride(Func<Guid, Guid, decimal, decimal> func)
        => _transactionSplitNetAmountOverride = func;

    /// <summary>
    /// Resets the override function for <see cref="TransactionSplitNetAmount"/>.
    /// </summary>
    internal static void ResetTransactionSplitNetAmountOverride()
        => _transactionSplitNetAmountOverride = null;

    /// <summary>
    /// Database function to calculate the net amount of a transaction split.
    /// </summary>
    /// <param name="transactionId">Transaction ID.</param>
    /// <param name="transactionSplitId">Transaction split ID.</param>
    /// <param name="amount">Split amount.</param>
    /// <returns>The net amount of the transaction split.</returns>
    /// <exception cref="NotSupportedException">Thrown if called outside of a database context operation and no override is set.</exception>
    [DbFunction("TransactionSplitNetAmount", "dbo")]
    public static decimal TransactionSplitNetAmount(Guid transactionId, Guid transactionSplitId, decimal amount)
        => _transactionSplitNetAmountOverride?.Invoke(transactionId, transactionSplitId, amount) ?? throw new NotSupportedException();
    #endregion
}
