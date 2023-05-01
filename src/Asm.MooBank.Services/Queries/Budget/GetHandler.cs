using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models.Queries.Budget;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Budget;

internal class GetHandler : QueryHandlerBase, IQueryHandler<Get, Models.BudgetLine>
{
    private readonly IQueryable<BudgetLine> _budgetLines;

    public GetHandler(IQueryable<BudgetLine> budgetLines, ISecurityRepository security) : base(security)
    {
        _budgetLines = budgetLines;
    }

    public async Task<Models.BudgetLine> Handle(Get request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await _budgetLines.SingleOrDefaultAsync(b => b.AccountId == request.AccountId && b.Id == request.Id, cancellationToken) ?? throw new NotFoundException();

        return entity;
    }
}