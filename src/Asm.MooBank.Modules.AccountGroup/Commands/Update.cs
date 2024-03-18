using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Modules.AccountGroup.Models;

namespace Asm.MooBank.Modules.AccountGroup.Commands;

public record Update(Models.AccountGroup AccountGroup) : ICommand<Models.AccountGroup>;

internal class UpdateHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.AccountGroup>
{
    public async ValueTask<Models.AccountGroup> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await accountGroupRepository.Get(request.AccountGroup.Id, cancellationToken);

        Security.AssertAccountGroupPermission(entity);

        entity.Name = request.AccountGroup.Name;
        entity.Description = request.AccountGroup.Description;
        entity.ShowPosition = request.AccountGroup.ShowPosition;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}

;
