using Asm.Cqrs.Commands;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public class CreateAccountGroup : ICommand<Models.AccountGroup>
{
    public Models.AccountGroup AccountGroup { get; private set; }

    public CreateAccountGroup(Models.AccountGroup accountGroup)
    {
        AccountGroup = accountGroup;
    }
}
