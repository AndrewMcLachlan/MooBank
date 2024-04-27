using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Commands;

public record Update(Models.Group AccountGroup) : ICommand<Models.Group>;

internal class UpdateHandler(IGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Group>
{
    public async ValueTask<Models.Group> Handle(Update request, CancellationToken cancellationToken)
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
