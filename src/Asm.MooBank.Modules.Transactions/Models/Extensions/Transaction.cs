using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;

namespace Asm.MooBank.Modules.Transactions;

public static class TransactionExtensions
{
    public static Transaction ToModel(this Domain.Entities.Transactions.Transaction transaction) =>
        new()
        {
            Id = transaction.Id,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            PurchaseDate = transaction.PurchaseDate,
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
            OffsetFor = transaction.OffsetFor.Select(t => t.ToOffsetForModel()),
        };

    public static Transaction ToSimpleModel(this Domain.Entities.Transactions.Transaction transaction) =>
        new()
        {
            Id = transaction.Id,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Notes = transaction.Notes,
        };

    public static IEnumerable<Transaction> ToModel(this IEnumerable<Domain.Entities.Transactions.Transaction> entities) =>
        entities.Select(t => t.ToModel());

    public static IQueryable<Transaction> ToModel(this IQueryable<Domain.Entities.Transactions.Transaction> entities) =>
        entities.Select(t => t.ToModel());

    public static async Task<IEnumerable<Transaction>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Transactions.Transaction>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(t => t.ToModel());
}