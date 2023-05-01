using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.Repositories
{
    internal class BudgetRepository : RepositoryDeleteBase<MooBankContext, BudgetLine, Guid>, IBudgetRepository
    {
        public BudgetRepository(MooBankContext context) : base(context)
        {
        }

        public override void Delete(Guid id)
        {
            Context.Remove(new BudgetLine(id));
        }
    }
}
