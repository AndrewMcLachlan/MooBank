using Asm.MooBank.Models;
using Asm.MooBank.Modules.Group.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Group.Queries;

public record GetAll : IQuery<IEnumerable<Models.Group>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Group.Group> accountGroups, User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.Group>>
{
    public async ValueTask<IEnumerable<Models.Group>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await accountGroups.Where(ag => ag.OwnerId == AccountHolder.Id).ToModel().ToListAsync(cancellationToken);
}
