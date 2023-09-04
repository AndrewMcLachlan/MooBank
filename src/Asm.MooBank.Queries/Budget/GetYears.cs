using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Budget;

public record GetYears() : IQuery<IEnumerable<short>>;

internal class GetYearsHandler : QueryHandlerBase, IQueryHandler<GetYears, IEnumerable<short>>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;

    public GetYearsHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, AccountHolder accountHolder) : base(accountHolder)
    {
        _budgets = budgets;
    }

    public async Task<IEnumerable<short>> Handle(GetYears request, CancellationToken cancellationToken)
    {
        Guid familyId = AccountHolder.FamilyId;

        return await _budgets.Where(b => b.FamilyId == familyId).Select(b => b.Year).ToListAsync(cancellationToken: cancellationToken);
    }
}
