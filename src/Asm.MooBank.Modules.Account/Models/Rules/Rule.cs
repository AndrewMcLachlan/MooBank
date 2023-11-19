using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Account.Models;

public partial record Rule
{
    public required int Id { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<Tag> Tags { get; set; }
}

public static class RuleExtensions
{
    public static Rule ToModel(this Domain.Entities.Account.Rule rule) =>
        new()
        {
            Id = rule.Id,
            Contains = rule.Contains,
            Description = rule.Description,
            Tags = rule.Tags.Where(t => t != null && !t.Deleted).Select(t => t.ToModel()),
        };

    public static IEnumerable<Rule> ToModel(this IEnumerable<Domain.Entities.Account.Rule> entities) =>
        entities.Select(ToModel);

    public static async Task<IEnumerable<Rule>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Account.Rule>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).ToModel();
    }
}