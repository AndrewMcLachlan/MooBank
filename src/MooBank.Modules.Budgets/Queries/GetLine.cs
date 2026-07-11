using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record GetLine(short Year, Guid Id) : IQuery<BudgetLine>;

internal class GetLineHandler(IQueryable<Domain.Entities.Budget.Budget> budgets) : IQueryHandler<GetLine, BudgetLine>
{
    public async ValueTask<BudgetLine> Handle(GetLine request, CancellationToken cancellationToken)
    {
        // The SoftDelete opt-out keeps the line visible when its tag is soft-deleted
        // (BudgetLine.Tag is a required navigation, so the filter would drop the whole line).
        var entity = await budgets
                   .Where(b => b.Year == request.Year)
                   .SelectMany(b => b.Lines)
                   .Include(l => l.Tag)
                   .IgnoreQueryFilters(["SoftDelete"])
                   .SingleOrDefaultAsync(l => l.Id == request.Id, cancellationToken) ?? throw new NotFoundException("Budget line not found");

        return entity.ToModel();
    }

}
