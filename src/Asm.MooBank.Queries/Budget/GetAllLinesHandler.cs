/*
using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Queries.Budget;

public record GetAllLines(Guid AccountId) : IQuery<IEnumerable<Models.BudgetLine>>;

internal class GetAllLinesHandler : QueryHandlerBase, IQueryHandler<GetAllLines, IEnumerable<Models.BudgetLine>>
{
    private readonly IQueryable<BudgetLine> _budgetLines;

    public GetAllLinesHandler(IQueryable<BudgetLine> budgetLines, ISecurity security) : base(security)
    {
        _budgetLines = budgetLines;
    }

    public Task<IEnumerable<Models.BudgetLine>> Handle(GetAllLines request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        return _budgetLines.Include(b => b.Tag).Where(b => b.AccountId == request.AccountId).ToModelAsync(cancellationToken);
    }
}

file static class IQueryableExtensions
{
    public static async Task<IEnumerable<Models.BudgetLine>> ToModelAsync(this IQueryable<BudgetLine> query, CancellationToken cancellationToken = default) =>
    await query.Select(t => (Models.BudgetLine)t).ToListAsync(cancellationToken);
}
*/