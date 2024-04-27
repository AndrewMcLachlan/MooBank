using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Modules.Groups.Commands;

public record Delete(Guid Id) : ICommand;

internal class DeleteHandler(IGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, Models.User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await accountGroupRepository.Get(request.Id, cancellationToken);

        Security.AssertAccountGroupPermission(group);

        accountGroupRepository.Delete(group);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
