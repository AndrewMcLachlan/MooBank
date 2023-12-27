using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class VirtualAccountRepository : RepositoryDeleteBase<VirtualAccount, Guid>, IVirtualAccountRepository
    {
        public VirtualAccountRepository(MooBankContext dataContext) : base(dataContext)
        {
        }

        protected override IQueryable<VirtualAccount> GetById(Guid id) =>  Entities.Where(v => v.Id == id);
    }
}
