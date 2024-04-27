using Asm.MooBank.Modules.Groups.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Groups.Queries;

public record Get(Guid Id) : IQuery<Group>;

internal class GetHandler(IQueryable<Domain.Entities.Group.Group> accountGroups, Models.User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Group>
{
    public async ValueTask<Group> Handle(Get request, CancellationToken cancellationToken)
    {
        var accountGroup = await accountGroups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == AccountHolder.Id, cancellationToken) ?? throw new NotFoundException($"Account Group with ID {request.Id} could not be found");

        return accountGroup.ToModel();
    }
}
