﻿namespace Asm.MooBank.Models;

public partial record Transaction
{
    public static explicit operator Transaction(Domain.Entities.Transactions.Transaction transaction) =>
        new()
        {
            Id = transaction.TransactionId,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Location = transaction.Location,
            AccountHolderName = transaction.AccountHolder?.FirstName,
            ExtraInfo = transaction.Extra,
            Notes = transaction.Notes,
            Tags = transaction.Tags.Where(t => !t.Deleted).ToSimpleModel(),
            Splits = transaction.Splits.Select(t => t.ToModel()),
            OffsetBy = transaction.OffsetBy.Select(t => t.ToOffsetByModel()),
            Offsets = transaction.Offsets.Select(t => t.ToOffsetModel()),

        };

   /* public static explicit operator Domain.Entities.Transactions.Transaction(Transaction transaction)
    {
        return new Domain.Entities.Transactions.Transaction
        {
            TransactionId = transaction.Id,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            Location = transaction.Location,

            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Notes = transaction.Notes,
        };
    }*/
}

public static class IEnumerableTransactionExtensions
{
    public static Transaction ToSimpleModel(this Domain.Entities.Transactions.Transaction transaction) =>
        new()
        {
            Id = transaction.TransactionId,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Notes = transaction.Notes,
        };

    public static IEnumerable<Transaction> ToModel(this IEnumerable<Domain.Entities.Transactions.Transaction> entities)
    {
        return entities.Select(t => (Transaction)t);
    }

    public static async Task<IEnumerable<Transaction>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Transactions.Transaction>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Transaction)t);
    }
}