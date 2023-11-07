﻿namespace Asm.MooBank.Modules.AccountGroup.Models;

public record AccountGroup
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool ShowPosition { get; init; }
}

public static class AccountGroupExtensions
{
    public static AccountGroup ToModel(this Domain.Entities.AccountGroup.AccountGroup entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ShowPosition = entity.ShowPosition,
        };



    public static IQueryable<AccountGroup> ToModel(this IQueryable<Domain.Entities.AccountGroup.AccountGroup> query) =>
        query.Select(t => t.ToModel());
}