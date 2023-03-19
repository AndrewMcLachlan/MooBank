using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class VirtualAccountRepository : RepositoryDeleteBase<VirtualAccount, Guid>, IVirtualAccountRepository
    {
        public VirtualAccountRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        protected override IQueryable<VirtualAccount> GetById(Guid id) =>  DataSet.Where(v => v.AccountId == id);
    }
}
