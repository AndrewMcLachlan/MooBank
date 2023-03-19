namespace Asm.MooBank.Models;

public partial record Transaction
{
    public static explicit operator Models.Transaction(Domain.Entities.Transaction transaction)
    {
        return new Models.Transaction
        {
            Id = transaction.TransactionId,
            Reference = transaction.TransactionReference,
            Amount = transaction.Amount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
            Tags = transaction.TransactionTags.Where(t => !t.Deleted).Select(t => (Models.TransactionTag)t),
        };
    }

    public static explicit operator Domain.Entities.Transaction(Models.Transaction transaction)
    {
        return new Domain.Entities.Transaction
        {
            //TransactionId = transaction.Id == Guid.Empty ? Guid.NewGuid() : transaction.Id,
            TransactionId = transaction.Id,
            TransactionReference = transaction.Reference,
            Amount = transaction.Amount,
            TransactionTime = transaction.TransactionTime,
            TransactionType = transaction.TransactionType,
            AccountId = transaction.AccountId,
            Description = transaction.Description,
        };
    }
}

public static class IEnumerableTransactionExtensions
{
    public static IEnumerable<Transaction> ToModel(this IEnumerable<Domain.Entities.Transaction> entities)
    {
        return entities.Select(t => (Transaction)t);
    }

    public static async Task<IEnumerable<Transaction>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Transaction>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Transaction)t);
    }
}