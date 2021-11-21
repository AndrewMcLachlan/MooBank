using System.Threading.Tasks;
using Asm.BankPlus.Data.Entities;

namespace Asm.BankPlus.Data.Repositories
{
    public interface IAccountHolderRepository
    {
        Task<AccountHolder> GetCurrent();
    }
}
