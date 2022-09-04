using System.Threading.Tasks;
using Asm.MooBank.Data.Entities;

namespace Asm.MooBank.Data.Repositories
{
    public interface IAccountHolderRepository
    {
        Task<AccountHolder> GetCurrent();
    }
}
