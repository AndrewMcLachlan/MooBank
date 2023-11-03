using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Account.Models;

public partial record Rule
{
    public required int Id { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<Tag> Tags { get; set; }
}

public partial record Rule
{
    public static implicit operator Rule(Domain.Entities.Account.Rule rule)
    {
        return new Rule
        {
            Id = rule.Id,
            Contains = rule.Contains,
            Description = rule.Description,
            Tags = rule.Tags.Where(t => t != null && !t.Deleted).Select(t => (Tag)t),
        };
    }

    public static implicit operator Domain.Entities.Account.Rule(Rule rule)
    {
        return new Domain.Entities.Account.Rule(rule.Id)
        {
            Contains = rule.Contains,
            Description = rule.Description,
            Tags = rule.Tags.Where(t => t != null).Select(t => (Domain.Entities.Tag.Tag)t).ToList(),
        };
    }
}


public static class IEnumerableRuleExtensions
{
    public static IEnumerable<Rule> ToModel(this IEnumerable<Domain.Entities.Account.Rule> entities)
    {
        return entities.Select(t => (Rule)t);
    }

    public static async Task<IEnumerable<Rule>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Account.Rule>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Rule)t);
    }
}