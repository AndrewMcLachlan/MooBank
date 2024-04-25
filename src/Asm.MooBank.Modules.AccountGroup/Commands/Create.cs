using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Modules.Group.Models;

namespace Asm.MooBank.Modules.Group.Commands;


public record Create(string Name, string Description, bool ShowPosition) : ICommand<Models.Group>;

internal class CreateHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Group>
{
    public async ValueTask<Models.Group> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.Group.Group entity = new()
        {
            Name = request.Name,
            Description = request.Description,
            ShowPosition = request.ShowPosition,
            OwnerId = AccountHolder.Id
        };

        groupRepository.Add(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}

