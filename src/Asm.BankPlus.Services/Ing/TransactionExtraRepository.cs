using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models.Ing;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Repository.Ing;

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
