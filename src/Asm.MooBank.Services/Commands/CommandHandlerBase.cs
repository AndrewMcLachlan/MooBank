using Asm.Domain;

namespace Asm.MooBank.Services.Commands;

public abstract class CommandHandlerBase
{
    protected IUnitOfWork UnitOfWork { get; }
    protected ISecurityRepository Security { get; }

    public CommandHandlerBase(IUnitOfWork unitOfWork, ISecurityRepository security)
    {
        UnitOfWork = unitOfWork;
        Security = security;
    }
}
