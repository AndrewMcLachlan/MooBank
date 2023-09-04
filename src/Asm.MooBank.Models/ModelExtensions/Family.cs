namespace Asm.MooBank.Models;
public static class FamilyExtensions
{
    public static Models.Family ToModel(this Domain.Entities.Family.Family family) =>
        new()
        {
            Id = family.Id,
            Name = family.Name,
            Members = family.AccountHolders.Select(ah => (AccountHolder)ah),
        };

    public static async Task<IEnumerable<Models.Family>> ToModelAsync(this Task<List<Domain.Entities.Family.Family>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(f => f.ToModel());
}

