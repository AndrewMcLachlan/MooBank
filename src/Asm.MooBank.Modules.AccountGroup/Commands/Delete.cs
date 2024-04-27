using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Modules.Groups.Commands;

public record Delete(Guid Id) : ICommand;

internal class DeleteHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork, ISecurity security) :  ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.Get(request.Id, cancellationToken);

        security.AssertGroupPermission(group);

        groupRepository.Delete(group);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
