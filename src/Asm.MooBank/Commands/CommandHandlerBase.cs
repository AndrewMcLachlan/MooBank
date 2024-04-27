using Asm.Domain;
using Asm.MooBank.Models;
using Asm.MooBank.Security;

namespace Asm.MooBank.Commands;

public abstract class CommandHandlerBase(IUnitOfWork unitOfWork, User accountHolder, ISecurity security)
{
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;
    protected ISecurity Security { get; } = security;

    protected User AccountHolder { get; } = accountHolder;
}
