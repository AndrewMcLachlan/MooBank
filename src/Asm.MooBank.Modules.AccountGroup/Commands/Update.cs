using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Modules.AccountGroup.Commands;

public record Update(Models.AccountGroup AccountGroup) : ICommand<Models.AccountGroup>;

internal class UpdateHandler : CommandHandlerBase, ICommandHandler<Update, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;

    public UpdateHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _accountGroupRepository = accountGroupRepository;
    }

    public async ValueTask<Models.AccountGroup> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await _accountGroupRepository.Get(request.AccountGroup.Id, cancellationToken);

        Security.AssertAccountGroupPermission(entity);

        entity.Name = request.AccountGroup.Name;
        entity.Description = request.AccountGroup.Description;
        entity.ShowPosition = request.AccountGroup.ShowPosition;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.AccountGroup)entity;
    }
}

;