using Asm.Domain;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Models.Commands.AccountGroup;

namespace Asm.MooBank.Services.Commands.AccountGroup;

internal class UpdateAccountGroupHandler : ICommandHandler<UpdateAccountGroup, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly ISecurityRepository _securityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountGroupHandler(IAccountGroupRepository accountGroupRepository, ISecurityRepository securityRepository, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _securityRepository = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.AccountGroup> Handle(UpdateAccountGroup request, CancellationToken cancellationToken)
    {
        var entity = await _accountGroupRepository.Get(request.AccountGroup.Id, cancellationToken);

        _securityRepository.AssertAccountGroupPermission(entity);

        entity.Name = request.AccountGroup.Name;
        entity.Description = request.AccountGroup.Description;
        entity.ShowPosition = request.AccountGroup.ShowPosition;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.AccountGroup)entity;
    }
}

