using Asm.MooBank.Models;
using Asm.MooBank.Modules.Family.Models;

namespace Asm.MooBank.Modules.Family.Queries;

public record GetAll() : IQuery<IEnumerable<Models.Family>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Family.Family> families, ISecurity security) : IQueryHandler<GetAll, IEnumerable<Models.Family>>
{
    public async ValueTask<IEnumerable<Models.Family>>Handle(GetAll request, CancellationToken cancellationToken)
    {
        security.AssertAdministrator();

        return await families.Include(f => f.AccountHolders).ToModel().ToListAsync(cancellationToken);
    }
}
