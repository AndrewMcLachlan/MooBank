using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Budget.Queries;

public record GetLine(short Year, Guid Id) : IQuery<BudgetLine>;

internal class GetLineHandler(IQueryable<Domain.Entities.Budget.BudgetLine> budgetLines, User accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<GetLine, BudgetLine>
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
