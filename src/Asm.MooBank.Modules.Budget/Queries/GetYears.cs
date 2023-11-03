using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Budget.Queries;

public record GetYears() : IQuery<IEnumerable<short>>;

internal class GetYearsHandler : QueryHandlerBase, IQueryHandler<GetYears, IEnumerable<short>>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;

    public GetYearsHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, AccountHolder accountHolder) : base(accountHolder)
    {
        _budgets = budgets;
    }

    public async ValueTask<IEnumerable<short>> Handle(GetYears request, CancellationToken cancellationToken)
    {
        Guid familyId = AccountHolder.FamilyId;

        return await _budgets.Where(b => b.FamilyId == familyId).Select(b => b.Year).ToListAsync(cancellationToken: cancellationToken);
    }
}
