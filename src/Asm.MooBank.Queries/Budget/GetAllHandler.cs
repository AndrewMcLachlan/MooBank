using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Queries.Budget;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Models.BudgetLine>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<Models.BudgetLine>>
{
    private readonly IQueryable<BudgetLine> _budgetLines;

    public GetAllHandler(IQueryable<BudgetLine> budgetLines, ISecurity security) : base(security)
    {
        _budgetLines = budgetLines;
    }

    public Task<IEnumerable<Models.BudgetLine>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        return _budgetLines.Include(b => b.Tag).Where(b => b.AccountId == request.AccountId).ToModelAsync(cancellationToken);
    }
}

file static class IQuerableExtensions
{
    public static async Task<IEnumerable<Models.BudgetLine>> ToModelAsync(this IQueryable<BudgetLine> query, CancellationToken cancellationToken = default) =>
    await query.Select(t => (Models.BudgetLine)t).ToListAsync(cancellationToken);
}