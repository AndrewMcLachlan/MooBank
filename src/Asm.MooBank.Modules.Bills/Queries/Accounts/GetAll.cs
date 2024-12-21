using Asm.Cqrs.Queries;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetAll() : IQuery<IEnumerable<Account>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user)  : IQueryHandler<GetAll, IEnumerable<Account>>
{
    public async ValueTask<IEnumerable<Account>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var all = await accounts.Where(a => a.Viewers.Any(ah => ah.UserId == userId)).ToListAsync(cancellationToken);

        return all.ToModel();
    }
}
