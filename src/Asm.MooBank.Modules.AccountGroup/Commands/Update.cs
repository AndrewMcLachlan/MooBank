using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Commands;

public record Update(Models.Group Group) : ICommand<Models.Group>;

internal class UpdateHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, Models.Group>
{
    public async ValueTask<Models.Group> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await groupRepository.Get(request.Group.Id, cancellationToken);

        security.AssertGroupPermission(entity);

        entity.Name = request.Group.Name;
        entity.Description = request.Group.Description;
        entity.ShowPosition = request.Group.ShowPosition;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}

;
