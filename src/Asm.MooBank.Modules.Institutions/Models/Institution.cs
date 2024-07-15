using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Institutions.Models;

public sealed record Institution
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required InstitutionType InstitutionType { get; init; }
}

public static class InstitutionExtensions
{
    public static Institution ToModel(this Domain.Entities.Institution.Institution institution) =>
        new()
        {
            Id = institution.Id,
            Name = institution.Name,
            InstitutionType = institution.InstitutionType,
        };

    public static IQueryable<Institution> ToModel(this IQueryable<Domain.Entities.Institution.Institution> query) =>
        query.Select(f => f.ToModel());
}

