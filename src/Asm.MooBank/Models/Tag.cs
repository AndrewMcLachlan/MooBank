namespace Asm.MooBank.Models;

public static class TagExtensions
{
    public static Tag ToModel(this Domain.Entities.Tag.Tag entity)
    {
        if (entity == null) return null!;
        return new Tag()
        {
            Id = entity.Id,
            Name = entity.Name,
            Colour = entity.Colour,
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
            Colour = tag.Colour,
            Tags = [.. tag.Tags.Select(t => t.ToEntity())],
            Settings = new Domain.Entities.Tag.TagSettings
            {
                ApplySmoothing = tag.Settings.ApplySmoothing,
                ExcludeFromReporting = tag.Settings.ExcludeFromReporting,
            }
        };
    public static ICollection<Domain.Entities.Tag.Tag> ToEntities(this IEnumerable<Tag> tags) =>
        [.. tags.Select(t => t.ToEntity())];

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
            Colour = entity.Colour,
            Tags = entity.Tags?.Select(t => t.ToHierarchyModel()) ?? [],
        };
}
