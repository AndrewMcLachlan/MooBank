using System.Data;
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.Repositories
{
    internal class BudgetRepository : RepositoryDeleteBase<MooBankContext, Budget, Guid>, IBudgetRepository
    {
        public BudgetRepository(MooBankContext context) : base(context)
        {
        }

        public BudgetLine AddLine(BudgetLine entity)
        {
            var result = Context.Add(entity).Entity;

            Context.Entry(result).Reference(e => e.Tag).Load();

            return result;
        }

        public override void Delete(Guid id)
        {
            Context.Remove(new Budget(id));
        }

        public void DeleteLine(Guid id)
        {
            Context.Remove(new BudgetLine(id));
        }

        public Task<Budget> GetByYear(Guid accountId, short year, CancellationToken cancellationToken = default) =>
            Entities.Where(b => b.AccountId == accountId && b.Year == year).SingleAsync(cancellationToken);

        public async Task<Budget> GetOrCreate(Guid accountId, short year, CancellationToken cancellationToken = default)
        {
            var budget = await Entities.Where(b => b.AccountId == accountId && b.Year == year).SingleOrDefaultAsync(cancellationToken);

            if (budget == null)
            {
                budget = new Budget(Guid.Empty)
                {
                    AccountId = accountId,
                    Year = year,
                };

                Entities.Add(budget);
            }

            return budget;
        }
    }
}
