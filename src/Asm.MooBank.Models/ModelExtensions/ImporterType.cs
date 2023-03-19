namespace Asm.MooBank.Models;

public partial record ImporterType
{
    public static explicit operator ImporterType(Domain.Entities.ImporterType entity)
    {
        return new ImporterType
        {
            Id = entity.ImporterTypeId,
            Type = entity.Type,
            Name = entity.Name,
        };
    }

    public static explicit operator Domain.Entities.ImporterType(ImporterType model)
    {
        return new Domain.Entities.ImporterType
        {
            ImporterTypeId = model.Id,
            Type = model.Type,
            Name = model.Name,
        };
    }
}

public static class IEnumerableImporterTypeExtensions
{
    public static IEnumerable<ImporterType> ToModel(this IEnumerable<Domain.Entities.ImporterType> entities)
    {
        return entities.Select(t => (ImporterType)t);
    }

    public static async Task<IEnumerable<ImporterType>> ToModelAsync(this Task<IEnumerable<Domain.Entities.ImporterType>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (ImporterType)t);
    }
}