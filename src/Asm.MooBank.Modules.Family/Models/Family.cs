using Asm.MooBank.Models;
using Asm.MooBank.Models.Extensions;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Models;
public record Family : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<User> Members { get; set; } = Enumerable.Empty<User>();
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

    public static IQueryable<Family> ToModel(this IQueryable<Domain.Entities.Family.Family> query) =>
        query.Select(f => f.ToModel());
}

