using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Family;

public record GetAll() : IQuery<IEnumerable<Models.Family>>;

internal class GetAllHandler : IQueryHandler<GetAll, IEnumerable<Models.Family>>
{
    private readonly IQueryable<Domain.Entities.Family.Family> _families;
    private readonly ISecurity _security;

    public GetAllHandler(IQueryable<Domain.Entities.Family.Family> families, ISecurity security)
    {
        _families = families;
        _security = security;
    }

    public Task<IEnumerable<Models.Family>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAdministrator();

        return _families.Include(f => f.AccountHolders).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
    }
}
