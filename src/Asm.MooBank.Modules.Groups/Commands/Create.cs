using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Commands;

[DisplayName("CreateGroup")]
public record Create(string Name, string Description, bool ShowPosition) : ICommand<Models.Group>;

internal class CreateHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork, MooBank.Models.User user) :  ICommandHandler<Create, Models.Group>
{
    public async ValueTask<Models.Group> Handle(Create request, CancellationToken cancellationToken)
    {
        // Security: Check not required the group is created against the current user.

        Domain.Entities.Group.Group entity = new()
        {
            Name = request.Name,
            Description = request.Description,
            ShowPosition = request.ShowPosition,
            OwnerId = user.Id
        };

        groupRepository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}

