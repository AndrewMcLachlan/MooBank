using Asm.Domain;

namespace Asm.MooBank.Models;
public record Family : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<AccountHolder> Members { get; set; } = Enumerable.Empty<AccountHolder>();
}
