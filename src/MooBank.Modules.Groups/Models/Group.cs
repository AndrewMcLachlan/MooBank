using System.ComponentModel;
using Asm.Drawing;

namespace Asm.MooBank.Modules.Groups.Models;

public record Group
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool ShowTotal { get; init; }
    public HexColour? Colour { get; init; }
}

/// <summary>
/// The order a user's groups should sit in, listed front to back.
/// </summary>
/// <remarks>
/// Wrapped in a record rather than sent as a bare list so it binds as a request body: a list of
/// identifiers on its own binds from the query string.
/// </remarks>
[DisplayName("GroupOrder")]
public sealed record GroupOrder
{
    public IEnumerable<Guid> GroupIds { get; init; } = [];
}

public static class GroupExtensions
{
    public static Group ToModel(this Domain.Entities.Group.Group entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ShowTotal = entity.ShowPosition,
            Colour = entity.Colour,
        };



    public static IQueryable<Group> ToModel(this IQueryable<Domain.Entities.Group.Group> query) =>
        query.Select(t => t.ToModel());
}
