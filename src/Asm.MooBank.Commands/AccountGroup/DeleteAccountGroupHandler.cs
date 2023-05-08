using Asm.Domain;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Models.Commands.AccountGroup;

namespace Asm.MooBank.Commands.AccountGroup;

internal class DeleteAccountGroupHandler : ICommandHandler<DeleteAccountGroup, bool>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly ISecurity _securityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountGroupHandler(IAccountGroupRepository accountGroupRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _securityRepository = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAccountGroup request, CancellationToken cancellationToken)
    {
        var group = await _accountGroupRepository.Get(request.Id, cancellationToken);

        _securityRepository.AssertAccountGroupPermission(group);

        _accountGroupRepository.Delete(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
