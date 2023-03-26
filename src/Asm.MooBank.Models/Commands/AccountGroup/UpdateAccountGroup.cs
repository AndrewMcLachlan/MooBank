using Asm.Cqrs.Commands;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public class UpdateAccountGroup : ICommand<Models.AccountGroup>
{
    public Models.AccountGroup AccountGroup { get; private set; }

    public UpdateAccountGroup(Models.AccountGroup accountGroup)
    {
        AccountGroup = accountGroup;
    }
}
