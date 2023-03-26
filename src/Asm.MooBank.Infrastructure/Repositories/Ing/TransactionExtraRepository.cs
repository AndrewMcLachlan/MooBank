using Asm.MooBank.Domain.Entities.Ing;

namespace Asm.MooBank.Infrastructure.Repositories.Ing
{
    public class TransactionExtraRepository : RepositoryBase<TransactionExtra, Guid>, ITransactionExtraRepository
    {
        public TransactionExtraRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public void AddRange(IEnumerable<TransactionExtra> transactions)
        {
            DataContext.AddRange(transactions);
        }

        protected override IQueryable<TransactionExtra> GetById(Guid id) => DataSet.Where(t => t.TransactionId == id);
    }
}
