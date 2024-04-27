using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Modules.Groups.Commands;

public record Delete(Guid Id) : ICommand;

internal class DeleteHandler(IGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, ISecurity security) :  ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await accountGroupRepository.Get(request.Id, cancellationToken);

        security.AssertAccountGroupPermission(group);

        accountGroupRepository.Delete(group);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
