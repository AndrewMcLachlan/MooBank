namespace Asm.MooBank.Models;

public partial record AccountGroup
{
    public static explicit operator AccountGroup(Domain.Entities.AccountGroup.AccountGroup entity)
    {
        return new AccountGroup
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ShowPosition = entity.ShowPosition,
        };
    }

    public static explicit operator Domain.Entities.AccountGroup.AccountGroup(AccountGroup model)
    {
        return new Domain.Entities.AccountGroup.AccountGroup
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ShowPosition = model.ShowPosition,
        };
    }
}

public static class IEnumerableAccountGroupExtensions
{
    public static IEnumerable<AccountGroup> ToModel(this IEnumerable<Domain.Entities.AccountGroup.AccountGroup> entities)
    {
        return entities.Select(t => (AccountGroup)t);
    }

    public static async Task<IEnumerable<AccountGroup>> ToModelAsync(this Task<IEnumerable<Domain.Entities.AccountGroup.AccountGroup>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (AccountGroup)t);
    }
}

public static class ListAccountGroupExtensions
{
    public static async Task<IEnumerable<AccountGroup>> ToModelAsync(this Task<List<Domain.Entities.AccountGroup.AccountGroup>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (AccountGroup)t);
    }
}