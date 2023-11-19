using Asm.MooBank.Modules.AccountGroup.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.AccountGroup.Queries;

public record Get(Guid Id) : IQuery<Models.AccountGroup>;

internal class GetHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, MooBank.Models.AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.AccountGroup>
{
    public async ValueTask<Models.AccountGroup> Handle(Get request, CancellationToken cancellationToken)
    {
        var accountGroup = (await accountGroups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == AccountHolder.Id, cancellationToken)) ?? throw new NotFoundException($"Account Group with ID {request.Id} could not be found");

        return accountGroup.ToModel();
    }
}
