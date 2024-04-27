using Asm.MooBank.Models;
using Asm.MooBank.Modules.Groups.Models;

namespace Asm.MooBank.Modules.Groups.Queries;

public record GetAll : IQuery<IEnumerable<Group>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Group.Group> accountGroups, User user) :IQueryHandler<GetAll, IEnumerable<Group>>
{
    public async ValueTask<IEnumerable<Group>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await accountGroups.Where(ag => ag.OwnerId == user.Id).ToModel().ToListAsync(cancellationToken);
}
