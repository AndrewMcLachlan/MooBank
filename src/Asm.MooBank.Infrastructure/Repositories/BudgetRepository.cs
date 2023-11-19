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
            var entity = Context.ChangeTracker.Entries<BudgetLine>().SingleOrDefault(e => e.Entity.Id == id)?.Entity ?? new BudgetLine(id);

            Context.Remove(entity);
        }


        // TODO: Get family ID from user data provider
        public Task<Budget> GetByYear(Guid familyId, short year, CancellationToken cancellationToken = default) =>
            Entities.Where(b => b.FamilyId == familyId && b.Year == year).SingleAsync(cancellationToken);

        public async Task<Budget> GetOrCreate(Guid familyId, short year, CancellationToken cancellationToken = default)
        {
            var budget = await Entities.Where(b => b.FamilyId == familyId && b.Year == year).SingleOrDefaultAsync(cancellationToken);

            if (budget == null)
            {
                budget = new Budget(Guid.Empty)
                {
                    FamilyId = familyId,
                    Year = year,
                };

                Entities.Add(budget);
            }

            return budget;
        }
    }
}
