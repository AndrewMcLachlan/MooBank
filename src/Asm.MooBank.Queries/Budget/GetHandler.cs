using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Queries.Budget;

public record Get(Guid AccountId, Guid Id) : IQuery<Models.BudgetLine>;

internal class GetHandler : QueryHandlerBase, IQueryHandler<Get, Models.BudgetLine>
{
    private readonly IQueryable<BudgetLine> _budgetLines;

    public GetHandler(IQueryable<BudgetLine> budgetLines, ISecurity security) : base(security)
    {
        _budgetLines = budgetLines;
    }

    public async Task<Models.BudgetLine> Handle(Get request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await _budgetLines.Include(b => b.Tag).SingleOrDefaultAsync(b => b.AccountId == request.AccountId && b.Id == request.Id, cancellationToken) ?? throw new NotFoundException();

        return entity;
    }
}