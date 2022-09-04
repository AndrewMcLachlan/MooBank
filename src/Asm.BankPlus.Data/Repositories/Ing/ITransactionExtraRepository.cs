using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Asm.MooBank.Models.Ing;

namespace Asm.MooBank.Data.Repositories.Ing
{
    public interface ITransactionExtraRepository
    {
        Task CreateTransactionExtras(IEnumerable<TransactionExtra> transactions);
    }
}
