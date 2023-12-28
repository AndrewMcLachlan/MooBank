using Asm.MooBank.Models;
using Asm.MooBank.Modules.Family.Models;

namespace Asm.MooBank.Modules.Family.Queries;

public record Get(Guid Id) : IQuery<Models.Family>;

internal class GetHandler(IQueryable<Domain.Entities.Family.Family> families, ISecurity security) : IQueryHandler<Get, Models.Family>
{
    private readonly IQueryable<Domain.Entities.Family.Family> _families = families;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Family> Handle(Get query, CancellationToken cancellationToken)
    {
        await _security.AssertAdministrator(cancellationToken);

        return await _families.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
    }
}
