using Asm.MooBank.Models;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Queries;

public record Get(Guid Id) : IQuery<Group>;

internal class GetHandler(IQueryable<Domain.Entities.Group.Group> groups, User user) : IQueryHandler<Get, Group>
{
    public async ValueTask<Group> Handle(Get request, CancellationToken cancellationToken)
    {
        var group = await groups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == user.Id, cancellationToken) ?? throw new NotFoundException($"Group with ID {request.Id} could not be found");

        return group.ToModel();
    }
}
