using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Budget.Specifications;

public class BudgetWithLinesSpecification : ISpecification<Budget>
{
    public IQueryable<Budget> Apply(IQueryable<Budget> query) =>
        query.Include(b => b.Lines);
}
