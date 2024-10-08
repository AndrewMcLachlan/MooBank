﻿namespace Asm.MooBank.Models;

public sealed record Tag
{
    private readonly TagSettings _settings = new();

    public int Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<Tag> Tags { get; set; } = Enumerable.Empty<Tag>();

    public TagSettings Settings
    {
        get => _settings;
        init
        {
            _settings = value ?? new();
        }
    }

    public partial record TagSettings
    {
        public bool ApplySmoothing { get; init; }

        public bool ExcludeFromReporting { get; init; }
    }
}

public static class TagExtensions
{
    public static Tag ToModel(this Domain.Entities.Tag.Tag entity)
    {
        if (entity == null) return null!;
        return new Tag()
        {
            Id = entity.Id,
            Name = entity.Name,
            Tags = entity.Tags.Where(t => t != null).Select(t => t.ToModel()).OrderBy(t => t.Name),
            Settings = new Tag.TagSettings
            {
                ApplySmoothing = entity.Settings.ApplySmoothing,
                ExcludeFromReporting = entity.Settings.ExcludeFromReporting,
            }
        };
    }

    public static Domain.Entities.Tag.Tag ToEntity(this Tag tag) =>
        new(tag.Id)
        {
            Name = tag.Name,
            Tags = tag.Tags.Select(t => t.ToEntity()).ToList(),
            Settings = new Domain.Entities.Tag.TagSettings
            {
                ApplySmoothing = tag.Settings.ApplySmoothing,
                ExcludeFromReporting = tag.Settings.ExcludeFromReporting,
            }
        };
    public static ICollection<Domain.Entities.Tag.Tag> ToEntities(this IEnumerable<Tag> tags) =>
        tags.Select(t => t.ToEntity()).ToArray();

    public static IEnumerable<Tag> ToModel(this IEnumerable<Domain.Entities.Tag.Tag> entities) =>
        entities.Select(t => t.ToModel());

    public static IQueryable<Tag> ToModel(this IQueryable<Domain.Entities.Tag.Tag> query) =>
        query.Select(t => t.ToModel());

    public static IEnumerable<Tag> ToSimpleModel(this IEnumerable<Domain.Entities.Tag.Tag> entities)
    {
        return entities.Select(t => new Tag()
        {
            Id = t.Id,
            Name = t.Name,
        });
    }

    public static async Task<IEnumerable<Tag>> ToHierarchyModelAsync(this Task<List<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(t => t.ToHierarchyModel());

    public static Tag ToHierarchyModel(this Domain.Entities.Tag.Tag entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Tags = entity.Tags?.Select(t => t.ToHierarchyModel()) ?? Enumerable.Empty<Tag>(),
        };
}
