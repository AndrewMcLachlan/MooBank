namespace Asm.MooBank.Models;

public partial record Transaction
{
    public static explicit operator Transaction(Domain.Entities.Transactions.Transaction transaction)
    {
        return new Transaction
        {
            Id = transaction.TransactionId,
            Reference = transaction.TransactionReference,
            Amount = transaction.Amount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Notes = transaction.Notes,
            Tags = transaction.TransactionTags.Where(t => !t.Deleted).ToSimpleModel(),
        };
    }

    public static explicit operator Domain.Entities.Transactions.Transaction(Transaction transaction)
    {
        return new Domain.Entities.Transactions.Transaction
        {
            TransactionId = transaction.Id,
            TransactionReference = transaction.Reference,
            Amount = transaction.Amount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Notes = transaction.Notes,
        };
    }
}

public static class IEnumerableTransactionExtensions
{
    public static IEnumerable<Transaction> ToModel(this IEnumerable<Domain.Entities.Transactions.Transaction> entities)
    {
        return entities.Select(t => (Transaction)t);
    }

    public static async Task<IEnumerable<Transaction>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Transactions.Transaction>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Transaction)t);
    }
}