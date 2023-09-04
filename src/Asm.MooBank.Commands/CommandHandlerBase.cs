using Asm.MooBank.Models;

namespace Asm.MooBank.Commands;

public abstract class CommandHandlerBase
{
    protected IUnitOfWork UnitOfWork { get; }
    protected ISecurity Security { get; }

    protected AccountHolder AccountHolder { get; }

    public CommandHandlerBase(IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security)
    {
        UnitOfWork = unitOfWork;
        AccountHolder = accountHolder;
        Security = security;
    }
}
