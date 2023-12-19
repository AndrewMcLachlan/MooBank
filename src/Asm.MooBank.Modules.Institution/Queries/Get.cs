using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institution.Models;

namespace Asm.MooBank.Modules.Institution.Queries;

public record Get(int Id) : IQuery<Models.Institution>;

internal class GetHandler(IQueryable<Domain.Entities.Institution.Institution> institutions, ISecurity security) : IQueryHandler<Get, Models.Institution>
{
    private readonly IQueryable<Domain.Entities.Institution.Institution> _institutions = institutions;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Institution> Handle(Get query, CancellationToken cancellationToken)
    {
        _security.AssertAdministrator();

        return await _institutions.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
    }
}
