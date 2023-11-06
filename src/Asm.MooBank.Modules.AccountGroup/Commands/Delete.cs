using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Modules.AccountGroup.Commands;

public record Delete(Guid Id) : ICommand;

internal class DeleteHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await accountGroupRepository.Get(request.Id, cancellationToken);

        Security.AssertAccountGroupPermission(group);

        accountGroupRepository.Delete(group);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
