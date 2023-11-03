using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.ReferenceData.Models;

public partial record ImporterType
{
    public int Id { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }
}



public partial record ImporterType
{
    public static explicit operator ImporterType(Domain.Entities.ReferenceData.ImporterType entity)
    {
        return new ImporterType
        {
            Id = entity.ImporterTypeId,
            Type = entity.Type,
            Name = entity.Name,
        };
    }

    public static explicit operator Domain.Entities.ReferenceData.ImporterType(ImporterType model)
    {
        return new Domain.Entities.ReferenceData.ImporterType
        {
            ImporterTypeId = model.Id,
            Type = model.Type,
            Name = model.Name,
        };
    }
}

public static class IEnumerableImporterTypeExtensions
{
    public static async Task<IEnumerable<ImporterType>> ToModelAsync(this IQueryable<Domain.Entities.ReferenceData.ImporterType> entities, CancellationToken cancellationToken = default) =>
        await entities.Select(t => (ImporterType)t).ToArrayAsync(cancellationToken);

    public static IEnumerable<ImporterType> ToModel(this IEnumerable<Domain.Entities.ReferenceData.ImporterType> entities)
    {
        return entities.Select(t => (ImporterType)t);
    }

    public static async Task<IEnumerable<ImporterType>> ToModelAsync(this Task<IEnumerable<Domain.Entities.ReferenceData.ImporterType>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (ImporterType)t);
    }
}