using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class VirtualAccountRepository(MooBankContext dataContext) : RepositoryDeleteBase<VirtualAccount, Guid>(dataContext), IVirtualAccountRepository
    {
        protected override IQueryable<VirtualAccount> GetById(Guid id) =>  Entities.Where(v => v.Id == id);
    }
}
