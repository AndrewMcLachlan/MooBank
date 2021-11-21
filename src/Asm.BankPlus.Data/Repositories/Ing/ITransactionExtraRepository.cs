using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Asm.BankPlus.Models.Ing;

namespace Asm.BankPlus.Data.Repositories.Ing
{
    public interface ITransactionExtraRepository
    {
        Task CreateTransactionExtras(IEnumerable<TransactionExtra> transactions);
    }
}
