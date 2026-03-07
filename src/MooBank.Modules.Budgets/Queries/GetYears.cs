using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record GetYears() : IQuery<IEnumerable<short>>;

internal class GetYearsHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, User user) : IQueryHandler<GetYears, IEnumerable<short>>
{
    public async ValueTask<IEnumerable<short>> Handle(GetYears request, CancellationToken cancellationToken)
    {
        Guid familyId = user.FamilyId;

        return await budgets.Where(b => b.FamilyId == familyId).Select(b => b.Year).ToListAsync(cancellationToken);
    }
}
