using System.Threading.Tasks;
using Asm.BankPlus.Data;

namespace Asm.BankPlus.Repository
{
    public interface IDataRepository
    {
        Task<int> SaveChanges();
    }

    public abstract class DataRepository : IDataRepository
    {
        protected BankPlusContext DataContext { get; }

        protected DataRepository(BankPlusContext dataContext)
        {
            DataContext = dataContext;
        }

        public Task<int> SaveChanges()
        {
            return DataContext.SaveChangesAsync();
        }
    }
}
