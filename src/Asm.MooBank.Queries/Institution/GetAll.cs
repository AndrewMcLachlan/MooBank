using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Institution;

public record GetAll() : IQuery<IEnumerable<Models.Institution>>;

internal class GetAllHandler : IQueryHandler<GetAll, IEnumerable<Models.Institution>>
{
    private readonly IQueryable<Domain.Entities.Institution.Institution> _institutions;
    private readonly ISecurity _security;

    public GetAllHandler(IQueryable<Domain.Entities.Institution.Institution> institutions, ISecurity security)
    {
        _institutions = institutions;
        _security = security;
    }

    public Task<IEnumerable<Models.Institution>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAdministrator();

        return _institutions.ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
    }
}
