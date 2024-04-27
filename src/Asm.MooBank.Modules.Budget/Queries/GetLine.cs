using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record GetLine(short Year, Guid Id) : IQuery<BudgetLine>;

internal class GetLineHandler(IQueryable<Domain.Entities.Budget.BudgetLine> budgetLines, ISecurity security) : IQueryHandler<GetLine, BudgetLine>
{
    public async ValueTask<BudgetLine> Handle(GetLine request, CancellationToken cancellationToken)
    {
        await security.AssertBudgetLinePermission(request.Id, cancellationToken);

        var entity = await budgetLines
                   .Include(b => b.Budget)
                   .Include(b => b.Tag)
                   .SingleOrDefaultAsync(b => b.Budget.Year == request.Year && b.Id == request.Id, cancellationToken) ?? throw new NotFoundException("Budget line not found");

        return entity.ToModel();
    }

}
