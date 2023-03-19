namespace Asm.MooBank.Models;

public partial record TransactionTag
{

    public static implicit operator Models.TransactionTag(Domain.Entities.TransactionTag transactionTag)
    {
        if (transactionTag == null) return null;
        return new Models.TransactionTag()
        {
            Id = transactionTag.TransactionTagId,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Where(t => t != null).Select(t => (Models.TransactionTag)t),
        };
    }

    public static implicit operator Domain.Entities.TransactionTag(Models.TransactionTag transactionTag)
    {
        return new Domain.Entities.TransactionTag
        {
            TransactionTagId = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Select(t => (Domain.Entities.TransactionTag)t).ToList(),
        };
    }
}


public static class IEnumerableTransactionTagExtensions
{
    public static IEnumerable<TransactionTag> ToModel(this IEnumerable<Domain.Entities.TransactionTag> entities)
    {
        return entities.Select(t => (TransactionTag)t);
    }

    public static async Task<IEnumerable<TransactionTag>> ToModelAsync(this Task<IEnumerable<Domain.Entities.TransactionTag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (TransactionTag)t);
    }
}