using Asm.Domain;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Family.Models;

namespace Asm.MooBank.Modules.Family.Models;
public record Family : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<AccountHolder> Members { get; set; } = Enumerable.Empty<AccountHolder>();
}

public static class FamilyExtensions
{
    public static Family ToModel(this Domain.Entities.Family.Family family) =>
        new()
        {
            Id = family.Id,
            Name = family.Name,
            Members = family.AccountHolders.Select(ah => ah.ToModel()),
        };

    public static async Task<IEnumerable<Family>> ToModelAsync(this Task<List<Domain.Entities.Family.Family>> entityTask, CancellationToken cancellationToken = default) =>
        (await entityTask.WaitAsync(cancellationToken)).Select(f => f.ToModel());
}

