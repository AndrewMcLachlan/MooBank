using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institution.Models;

namespace Asm.MooBank.Modules.Institution.Queries;

public record GetAll() : IQuery<IEnumerable<Models.Institution>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Institution.Institution> institutions, ISecurity security) : IQueryHandler<GetAll, IEnumerable<Models.Institution>>
{
    private readonly IQueryable<Domain.Entities.Institution.Institution> _institutions = institutions;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Institution>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAdministrator();

        return await _institutions.ToModel().ToListAsync(cancellationToken);
    }
}
