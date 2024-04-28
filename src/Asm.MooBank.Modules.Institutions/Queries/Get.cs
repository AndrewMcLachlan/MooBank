using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Queries;

public record Get(int Id) : IQuery<Institution>;

internal class GetHandler(IQueryable<Domain.Entities.Institution.Institution> institutions) : IQueryHandler<Get, Institution>
{
    public async ValueTask<Institution> Handle(Get query, CancellationToken cancellationToken) =>
        await institutions.Where(i => i.Id == query.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
}
