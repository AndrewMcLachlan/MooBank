namespace Asm.MooBank.Models;

public partial record TransactionTag
{

    public static implicit operator TransactionTag(Domain.Entities.TransactionTags.TransactionTag transactionTag)
    {
        if (transactionTag == null) return null!;
        return new TransactionTag()
        {
            Id = transactionTag.TransactionTagId,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Where(t => t != null).Select(t => (TransactionTag)t),
        };
    }

    public static implicit operator Domain.Entities.TransactionTags.TransactionTag(TransactionTag transactionTag)
    {
        return new Domain.Entities.TransactionTags.TransactionTag
        {
            TransactionTagId = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Select(t => (Domain.Entities.TransactionTags.TransactionTag)t).ToList(),
        };
    }
}


public static class IEnumerableTransactionTagExtensions
{
    public static IEnumerable<TransactionTag> ToModel(this IEnumerable<Domain.Entities.TransactionTags.TransactionTag> entities)
    {
        return entities.Select(t => (TransactionTag)t);
    }

    public static async Task<IEnumerable<TransactionTag>> ToModelAsync(this Task<IEnumerable<Domain.Entities.TransactionTags.TransactionTag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (TransactionTag)t);
    }
}