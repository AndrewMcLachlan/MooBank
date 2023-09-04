namespace Asm.MooBank.Models;
public static class InstitutionExtensions
{
    public static Models.Institution ToModel(this Domain.Entities.Institution.Institution institution) =>
        new()
        {
            Id = institution.Id,
            Name = institution.Name,
        };

    public static async Task<IEnumerable<Models.Institution>> ToModelAsync(this Task<List<Domain.Entities.Institution.Institution>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(f => f.ToModel());
}

