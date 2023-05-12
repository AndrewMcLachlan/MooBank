using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.Repositories
{
    internal class BudgetRepository : RepositoryDeleteBase<MooBankContext, BudgetLine, Guid>, IBudgetRepository
    {
        public BudgetRepository(MooBankContext context) : base(context)
        {
        }

        public override BudgetLine Add(BudgetLine entity)
        {
            var result = base.Add(entity);

            Context.Entry(result).Reference(e => e.Tag).Load();

            return result;
        }

        public override void Delete(Guid id)
        {
            Context.Remove(new BudgetLine(id));
        }
    }
}
