using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Budget;

public interface IBudgetRepository : IDeleteRepository<BudgetLine, Guid>, IRepository<BudgetLine, Guid>
{

}
