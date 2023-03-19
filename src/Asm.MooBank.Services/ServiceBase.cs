using Asm.Domain;

namespace Asm.MooBank.Services;

public class ServiceBase
{
    protected IUnitOfWork UnitOfWork { get; }

    public ServiceBase(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }
}
