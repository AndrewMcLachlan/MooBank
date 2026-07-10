using Asm.MooBank.Models;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Queries;

public record Get(Guid Id) : IQuery<Family>;

internal class GetHandler(IQueryable<Domain.Entities.Family.Family> families) : IQueryHandler<Get, Family>
{
    public async ValueTask<Family> Handle(Get query, CancellationToken cancellationToken) =>
        await families.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
}
