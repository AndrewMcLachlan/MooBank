using Asm.MooBank.Models;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Queries;

public record Get(Guid Id) : IQuery<Family>;

internal class GetHandler(IQueryable<Domain.Entities.Family.Family> families, ISecurity security) : IQueryHandler<Get, Family>
{
    private readonly IQueryable<Domain.Entities.Family.Family> _families = families;
    private readonly ISecurity _security = security;

    public async ValueTask<Family> Handle(Get query, CancellationToken cancellationToken)
    {
        await _security.AssertAdministrator(cancellationToken);

        return await _families.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
    }
}
