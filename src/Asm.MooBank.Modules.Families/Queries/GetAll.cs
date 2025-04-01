using Asm.MooBank.Models;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Queries;

public record GetAll() : IQuery<IEnumerable<Family>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Family.Family> families, ISecurity security) : IQueryHandler<GetAll, IEnumerable<Family>>
{
    public async ValueTask<IEnumerable<Family>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator(cancellationToken);

        return await families.Include(f => f.AccountHolders).OrderBy(f => f.Name).ToModel().ToListAsync(cancellationToken);
    }
}
