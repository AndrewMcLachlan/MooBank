using Asm.Cqrs.Commands;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Models;
using Asm.MooBank.Models.Commands.AccountGroup;
using Asm.MooBank.Security;

namespace Asm.MooBank.Services.Commands.AccountGroup;

internal class CreateAccountGroupHandler : ICommandHandler<CreateAccountGroup, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly IUserDataProvider _userDataProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountGroupHandler(IAccountGroupRepository accountGroupRepository, IUserDataProvider userDataProvider, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _userDataProvider = userDataProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.AccountGroup> Handle(CreateAccountGroup request, CancellationToken cancellationToken)
    {
        var entity = (Domain.Entities.AccountGroup.AccountGroup)request.AccountGroup;

        entity.OwnerId = _userDataProvider.CurrentUserId;

        _accountGroupRepository.Add(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.AccountGroup)entity;
    }
}

