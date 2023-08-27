namespace Asm.MooBank.Models;

public partial record TransactionTagRule
{
    public static implicit operator TransactionTagRule(Domain.Entities.Account.TransactionTagRule rule)
    {
        return new Models.TransactionTagRule
        {
            Id = rule.TransactionTagRuleId,
            Contains = rule.Contains,
            Description = rule.Description,
            Tags = rule.TransactionTags.Where(t => t != null && !t.Deleted).Select(t => (TransactionTag)t),
        };
    }

    public static implicit operator Domain.Entities.Account.TransactionTagRule(TransactionTagRule rule)
    {
        return new Domain.Entities.Account.TransactionTagRule
        {
            TransactionTagRuleId = rule.Id,
            Contains = rule.Contains,
            Description = rule.Description,
            TransactionTags = rule.Tags.Where(t=> t != null).Select(t => (Domain.Entities.Tag.Tag)t).ToList(),
        };
    }
}

public static class IEnumerableTransactionTagRuleExtensions
{
    public static IEnumerable<TransactionTagRule> ToModel(this IEnumerable<Domain.Entities.Account.TransactionTagRule> entities)
    {
        return entities.Select(t => (TransactionTagRule)t);
    }

    public static async Task<IEnumerable<TransactionTagRule>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Account.TransactionTagRule>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (TransactionTagRule)t);
    }
}