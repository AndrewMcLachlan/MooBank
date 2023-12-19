using Asm.MooBank.Models;
using Asm.MooBank.Modules.Family.Models;

namespace Asm.MooBank.Modules.Family.Queries;

public record Get(Guid Id) : IQuery<Models.Family>;

internal class GetHandler(IQueryable<Domain.Entities.Family.Family> Familys, ISecurity security) : IQueryHandler<Get, Models.Family>
{
    private readonly IQueryable<Domain.Entities.Family.Family> _Familys = Familys;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Family> Handle(Get query, CancellationToken cancellationToken)
    {
        _security.AssertAdministrator();

        return await _Familys.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
    }
}
