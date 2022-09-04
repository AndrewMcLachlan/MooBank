using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Data.Repositories
{
    public interface ITransactionTagRepository
    {
        Task<TransactionTag> Create(TransactionTag tag);

        Task<TransactionTag> Create(string name);

        Task<TransactionTag> Update(int id, string name);

        Task<IEnumerable<TransactionTag>> Get();

        Task<IEnumerable<Data.Entities.TransactionTag>> Get(IEnumerable<int> tagIds);

        Task<TransactionTag> Get(int id);

        Task Delete(int id);

        Task<TransactionTag> AddSubTag(int id, int subId);

        Task RemoveSubTag(int id, int subId);

    }
}
