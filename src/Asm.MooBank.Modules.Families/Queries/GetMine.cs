using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Queries;

public record GetMine() : IQuery<Family>;

internal class GetMineHandler(IQueryable<Domain.Entities.Family.Family> families, MooBank.Models.User user) : IQueryHandler<GetMine, Family>
{
    public async ValueTask<Family> Handle(GetMine query, CancellationToken cancellationToken)
    {
        return await families
            .Include(f => f.AccountHolders)
            .Where(f => f.Id == user.FamilyId)
            .ToModel()
            .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
    }
}
