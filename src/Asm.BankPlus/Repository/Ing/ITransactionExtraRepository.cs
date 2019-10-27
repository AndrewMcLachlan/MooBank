using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Asm.BankPlus.Models.Ing;

namespace Asm.BankPlus.Repository.Ing
{
    public interface ITransactionExtraRepository
    {
        Task CreateTransactionExtras(IEnumerable<TransactionExtra> transactions);
    }
}
