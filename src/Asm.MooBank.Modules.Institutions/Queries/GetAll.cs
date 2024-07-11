using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Queries;

public record GetAll() : IQuery<IEnumerable<Institution>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Institution.Institution> institutions) : IQueryHandler<GetAll, IEnumerable<Institution>>
{

    public async ValueTask<IEnumerable<Institution>> Handle(GetAll request, CancellationToken cancellationToken) =>
        await institutions.OrderBy(x => x.Name).ToModel().ToListAsync(cancellationToken);
}
