namespace Asm.MooBank.Modules.ReferenceData.Models;
public sealed record InstitutionType
{
    public int Id { get; set; }

    public required string Name { get; set; }
}

public static class InstitutionTypeExtensions
{
    public static InstitutionType ToModel(this Domain.Entities.Institution.InstitutionType entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
        };

    public static IQueryable<InstitutionType> ToModel(this IQueryable<Domain.Entities.Institution.InstitutionType> query) =>
        query.Select(t => t.ToModel());
}