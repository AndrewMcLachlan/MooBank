using Asm.Domain;

namespace Asm.MooBank.Services.Commands;

public abstract class CommandHandlerBase
{
    protected IUnitOfWork UnitOfWork { get; }
    protected ISecurity Security { get; }

    public CommandHandlerBase(IUnitOfWork unitOfWork, ISecurity security)
    {
        UnitOfWork = unitOfWork;
        Security = security;
    }
}
