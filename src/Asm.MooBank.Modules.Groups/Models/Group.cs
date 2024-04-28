namespace Asm.MooBank.Modules.Groups.Models;

public record Group
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool ShowPosition { get; init; }
}

public static class GroupExtensions
{
    public static Group ToModel(this Domain.Entities.Group.Group entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ShowPosition = entity.ShowPosition,
        };



    public static IQueryable<Group> ToModel(this IQueryable<Domain.Entities.Group.Group> query) =>
        query.Select(t => t.ToModel());
}
