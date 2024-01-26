using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institution.Models;

namespace Asm.MooBank.Modules.Institution.Queries;

public record GetAll() : IQuery<IEnumerable<Models.Institution>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Institution.Institution> institutions) : IQueryHandler<GetAll, IEnumerable<Models.Institution>>
{

    public async ValueTask<IEnumerable<Models.Institution>> Handle(GetAll request, CancellationToken cancellationToken) =>
        await institutions.ToModel().ToListAsync(cancellationToken);
}
