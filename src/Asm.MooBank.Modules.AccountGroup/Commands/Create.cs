using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Modules.AccountGroup.Models;

namespace Asm.MooBank.Modules.AccountGroup.Commands;


public record Create(string Name, string Description, bool ShowPosition) : ICommand<Models.AccountGroup>;

internal class CreateHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.AccountGroup>
{
    public async ValueTask<Models.AccountGroup> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.AccountGroup.AccountGroup entity = new()
        {
            Name = request.Name,
            Description = request.Description,
            ShowPosition = request.ShowPosition,
            OwnerId = AccountHolder.Id
        };

        accountGroupRepository.Add(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}

