namespace Asm.MooBank.Modules.ReferenceData.Models;

public sealed record ImporterType
{
    public int Id { get; set; }

    public required string Type { get; set; }

    public required string Name { get; set; }
}

public static class ImporterTypeExtensions
{
    public static ImporterType ToModel(this Domain.Entities.ReferenceData.ImporterType entity) =>
        new()
        {
            Id = entity.ImporterTypeId,
            Type = entity.Type,
            Name = entity.Name,
        };

    public static IQueryable<ImporterType> ToModel(this IQueryable<Domain.Entities.ReferenceData.ImporterType> query) =>
        query.Select(t => t.ToModel());
}