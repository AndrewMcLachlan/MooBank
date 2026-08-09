using Asm.MooBank.Models;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Queries;

public record GetAll : IQuery<IEnumerable<Group>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Group.Group> groups, User user) : IQueryHandler<GetAll, IEnumerable<Group>>
{
    // Name breaks ties so the list cannot shuffle between requests: groups created before the
    // order existed all share a sort order until something reorders them.
    public async ValueTask<IEnumerable<Group>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await groups.Where(ag => ag.OwnerId == user.Id)
                    .OrderBy(ag => ag.SortOrder).ThenBy(ag => ag.Name)
                    .ToModel().ToListAsync(cancellationToken);
}
