namespace Asm.MooBank.Models;

public partial record Tag
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



public partial record Tag
{

    public static implicit operator Tag(Domain.Entities.Tag.Tag transactionTag)
    {
        if (transactionTag == null) return null!;
        return new Tag()
        {
            Id = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Where(t => t != null).Select(t => (Tag)t).OrderBy(t => t.Name),
            Settings = transactionTag.Settings
        };
    }

    public static implicit operator Domain.Entities.Tag.Tag(Tag transactionTag)
    {
        return new Domain.Entities.Tag.Tag
        {
            Id = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Select(t => (Domain.Entities.Tag.Tag)t).ToList(),
            Settings = new Domain.Entities.Tag.TagSettings
            {
                ApplySmoothing = transactionTag.Settings.ApplySmoothing,
                ExcludeFromReporting = transactionTag.Settings.ExcludeFromReporting,
            }
        };
    }


    public partial record TagSettings
    {
        public static implicit operator TagSettings(Domain.Entities.Tag.TagSettings? settings)
        {
            if (settings == null) return null!;

            return new TagSettings
            {
                ApplySmoothing = settings.ApplySmoothing,
                ExcludeFromReporting = settings.ExcludeFromReporting,
            };
        }

        public static implicit operator Domain.Entities.Tag.TagSettings(TagSettings? settings)
        {
            if (settings == null) return null!;

            return new Domain.Entities.Tag.TagSettings()
            {
                ApplySmoothing = settings.ApplySmoothing,
                ExcludeFromReporting = settings.ExcludeFromReporting,
            };
        }
    }
}

public static class TagExtensions
{
    public static Tag ToModel(this Domain.Entities.Tag.Tag entity) => (Tag)entity;

    public static Domain.Entities.Tag.Tag ToEntity(this Tag tag) =>
        new()
        {
            Id = tag.Id,
            Name = tag.Name,
            Tags = tag.Tags.Select(t => (Domain.Entities.Tag.Tag)t).ToList(),
            Settings = new Domain.Entities.Tag.TagSettings
            {
                ApplySmoothing = tag.Settings.ApplySmoothing,
                ExcludeFromReporting = tag.Settings.ExcludeFromReporting,
            }
        };
    }

public static class IEnumerableTransactionTagExtensions
{
    public static IEnumerable<Tag> ToModel(this IEnumerable<Domain.Entities.Tag.Tag> entities)
    {
        return entities.Select(t => (Tag)t);
    }

    public static async Task<IEnumerable<Tag>> ToModelAsync(this Task<List<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Tag)t);
    }

    public static async Task<IEnumerable<Tag>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (Tag)t);
    }

    public static IEnumerable<Tag> ToSimpleModel(this IEnumerable<Domain.Entities.Tag.Tag> entities)
    {
        return entities.Select(t => new Tag()
        {
            Id = t.Id,
            Name = t.Name,
        });
    }

    public static async Task<IEnumerable<Tag>> ToHierarchyModelAsync(this Task<List<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => t.ToHierarchyModel());
    }

    public static Tag ToHierarchyModel(this Domain.Entities.Tag.Tag entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Tags = entity.Tags?.Select(t => t.ToHierarchyModel()) ?? Enumerable.Empty<Tag>(),
        };
}