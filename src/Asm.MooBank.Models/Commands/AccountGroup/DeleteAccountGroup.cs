using Asm.Cqrs.Commands;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public record DeleteAccountGroup : ICommand<bool>
{
    public Guid Id { get; init; }

    public DeleteAccountGroup(Guid id)
    {
        Id = id;
    }

}
