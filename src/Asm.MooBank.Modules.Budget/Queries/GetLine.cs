using Asm.MooBank.Models;
using Asm.MooBank.Queries;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Queries;

public record GetLine(short Year, Guid Id) : IQuery<BudgetLine>;

internal class GetLineHandler : QueryHandlerBase, IQueryHandler<GetLine, BudgetLine>
{
    private readonly IQueryable<Domain.Entities.Budget.BudgetLine> _budgetLines;
    private readonly ISecurity _security;

    public GetLineHandler(IQueryable<Domain.Entities.Budget.BudgetLine> budgetLines, AccountHolder accountHolder, ISecurity security) : base(accountHolder)
    {
        _budgetLines = budgetLines;
        _security = security;
    }

    public async ValueTask<BudgetLine> Handle(GetLine request, CancellationToken cancellationToken)
    {
        await _security.AssertBudgetLinePermission(request.Id, cancellationToken);

        return await _budgetLines
                   .Include(b => b.Budget)
                   .Include(b => b.Tag)
                   .SingleOrDefaultAsync(b => b.Budget.Year == request.Year && b.Id == request.Id, cancellationToken) ?? throw new NotFoundException("Budget line not found");
    }

}
