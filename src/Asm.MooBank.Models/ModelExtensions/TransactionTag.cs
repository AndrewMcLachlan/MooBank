namespace Asm.MooBank.Models;

public partial record TransactionTag
{

    public static implicit operator TransactionTag(Domain.Entities.Tag.Tag transactionTag)
    {
        if (transactionTag == null) return null!;
        return new TransactionTag()
        {
            Id = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Where(t => t != null).Select(t => (TransactionTag)t).OrderBy(t => t.Name),
            Settings = transactionTag.Settings
        };
    }

    public static implicit operator Domain.Entities.Tag.Tag(TransactionTag transactionTag)
    {
        return new Domain.Entities.Tag.Tag
        {
            Id = transactionTag.Id,
            Name = transactionTag.Name,
            Tags = transactionTag.Tags.Select(t => (Domain.Entities.Tag.Tag)t).ToList(),
            Settings = new Domain.Entities.Tag.TransactionTagSettings
            {
                ApplySmoothing = transactionTag.Settings.ApplySmoothing,
                ExcludeFromReporting = transactionTag.Settings.ExcludeFromReporting,
            }
        };
    }


    public partial record TransactionTagSettings
    {
        public static implicit operator TransactionTagSettings(Domain.Entities.Tag.TransactionTagSettings? settings)
        {
            if (settings == null) return null!;

            return new TransactionTagSettings
            {
                ApplySmoothing = settings.ApplySmoothing,
                ExcludeFromReporting = settings.ExcludeFromReporting,
            };
        }

        public static implicit operator Domain.Entities.Tag.TransactionTagSettings(TransactionTagSettings? settings)
        {
            if (settings == null) return null!;

            return new Domain.Entities.Tag.TransactionTagSettings()
            {
                ApplySmoothing = settings.ApplySmoothing,
                ExcludeFromReporting = settings.ExcludeFromReporting,
            };
        }
    }
}

public static class IEnumerableTransactionTagExtensions
{
    public static IEnumerable<TransactionTag> ToModel(this IEnumerable<Domain.Entities.Tag.Tag> entities)
    {
        return entities.Select(t => (TransactionTag)t);
    }


    public static async Task<IEnumerable<TransactionTag>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (TransactionTag)t);
    }

    public static IEnumerable<TransactionTag> ToSimpleModel(this IEnumerable<Domain.Entities.Tag.Tag> entities)
    {
        return entities.Select(t => new TransactionTag()
        {
            Id = t.Id,
            Name = t.Name,
        });
    }

    public static async Task<IEnumerable<TransactionTag>> ToHierarchyModelAsync(this Task<List<Domain.Entities.Tag.Tag>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => t.ToHierarchyModel());
    }

    public static TransactionTag ToHierarchyModel(this Domain.Entities.Tag.Tag entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Tags = entity.Tags?.Select(t => t.ToHierarchyModel()) ?? Enumerable.Empty<TransactionTag>(),
        };
}