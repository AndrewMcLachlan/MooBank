using Asm.MooBank.Modules.Group.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Group.Queries;

public record Get(Guid Id) : IQuery<Models.Group>;

internal class GetHandler(IQueryable<Domain.Entities.Group.Group> accountGroups, MooBank.Models.AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Group>
{
    public async ValueTask<Models.Group> Handle(Get request, CancellationToken cancellationToken)
    {
        var accountGroup = (await accountGroups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == AccountHolder.Id, cancellationToken)) ?? throw new NotFoundException($"Account Group with ID {request.Id} could not be found");

        return accountGroup.ToModel();
    }
}
