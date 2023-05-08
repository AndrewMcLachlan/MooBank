namespace Asm.MooBank.Commands;

internal abstract class DataCommandHandler
{
    protected IUnitOfWork UnitOfWork { get; private set; }

    protected DataCommandHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }
}
