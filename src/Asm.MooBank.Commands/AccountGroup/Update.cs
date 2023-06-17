using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Models.Commands.AccountGroup;

public record Update(Models.AccountGroup AccountGroup) : ICommand<Models.AccountGroup>;

internal class UpdateHandler : ICommandHandler<Update, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;
    private readonly ISecurity _securityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHandler(IAccountGroupRepository accountGroupRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _accountGroupRepository = accountGroupRepository;
        _securityRepository = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.AccountGroup> Handle(Update request, CancellationToken cancellationToken)
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

;