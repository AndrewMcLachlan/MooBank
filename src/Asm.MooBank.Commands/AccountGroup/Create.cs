using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public record Create(Models.AccountGroup AccountGroup) : ICommand<Models.AccountGroup>;

internal class CreateHandler : ICommandHandler<Create, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly IUserDataProvider _userDataProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHandler(IAccountGroupRepository accountGroupRepository, IUserDataProvider userDataProvider, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _userDataProvider = userDataProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.AccountGroup> Handle(Create request, CancellationToken cancellationToken)
    {
        var entity = (Domain.Entities.AccountGroup.AccountGroup)request.AccountGroup;

        entity.OwnerId = _userDataProvider.CurrentUserId;

        _accountGroupRepository.Add(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.AccountGroup)entity;
    }
}

