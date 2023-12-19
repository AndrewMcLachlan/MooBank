namespace Asm.MooBank.Modules.Institution.Models;

public sealed record Institution
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int InstitutionTypeId { get; init; }
}

public static class InstitutionExtensions
{
    public static Institution ToModel(this Domain.Entities.Institution.Institution institution) =>
        new()
        {
            Id = institution.Id,
            Name = institution.Name,
            InstitutionTypeId = institution.InstitutionTypeId,
        };

    public static IQueryable<Institution> ToModel(this IQueryable<Domain.Entities.Institution.Institution> query) =>
        query.Select(f => f.ToModel());
}

