namespace Asm.MooBank.Modules.Institution.Models;

public record Institution
{
    public required int Id { get; init; }

    public required string Name { get; init; }
}

public static class InstitutionExtensions
{
    public static Institution ToModel(this Domain.Entities.Institution.Institution institution) =>
        new()
        {
            Id = institution.Id,
            Name = institution.Name,
        };

    public static async Task<IEnumerable<Institution>> ToModelAsync(this Task<List<Domain.Entities.Institution.Institution>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(f => f.ToModel());
}

