using Asm.BankPlus.Data.Repositories.Ing;
using Asm.BankPlus.Models.Ing;

namespace Asm.BankPlus.Services.Ing
{
    public class TransactionExtraRepository : DataRepository, ITransactionExtraRepository
    {
        public TransactionExtraRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task CreateTransactionExtras(IEnumerable<TransactionExtra> transactions)
        {
            var entities = transactions.Select(t => (Data.Entities.Ing.TransactionExtra)t).ToList();

            DataContext.AddRange(entities);

            await DataContext.SaveChangesAsync();
        }
    }
}
