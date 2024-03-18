using Asm.MooBank.Models;
using Asm.MooBank.Modules.AccountGroup.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.AccountGroup.Queries;

public record GetAll : IQuery<IEnumerable<Models.AccountGroup>>;

internal class GetAllHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.AccountGroup>>
{
    public async ValueTask<IEnumerable<Models.AccountGroup>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await accountGroups.Where(ag => ag.OwnerId == AccountHolder.Id).ToModel().ToListAsync(cancellationToken);
}
