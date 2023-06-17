using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public record Delete(Guid Id) : ICommand<bool>;

internal class DeleteHandler : ICommandHandler<Delete, bool>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly ISecurity _securityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHandler(IAccountGroupRepository accountGroupRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _securityRepository = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await _accountGroupRepository.Get(request.Id, cancellationToken);

        _securityRepository.AssertAccountGroupPermission(group);

        _accountGroupRepository.Delete(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
