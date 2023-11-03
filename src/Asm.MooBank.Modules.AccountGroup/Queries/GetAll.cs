using Asm.MooBank.Models;
using Asm.MooBank.Queries;
using Asm.MooBank.Modules.AccountGroup.Models;

namespace Asm.MooBank.Modules.AccountGroup.Queries;

public record GetAll : IQuery<IEnumerable<Models.AccountGroup>>;

internal class GetAllHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.AccountGroup>>
{
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups = accountGroups;

    public async ValueTask<IEnumerable<Models.AccountGroup>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await _accountGroups.Where(ag => ag.OwnerId == AccountHolder.Id).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
