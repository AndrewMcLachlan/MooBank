using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institution.Models;

namespace Asm.MooBank.Modules.Institution.Queries;

public record Get(int Id) : IQuery<Models.Institution>;

internal class GetHandler(IQueryable<Domain.Entities.Institution.Institution> institutions) : IQueryHandler<Get, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Get query, CancellationToken cancellationToken) =>
        await institutions.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
}
