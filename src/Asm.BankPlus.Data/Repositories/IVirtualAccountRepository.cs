using System;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Data.Repositories
{
    public interface IVirtualAccountRepository
    {
        Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId);

        Task<VirtualAccount> Create(Guid accountId, VirtualAccount account);

        Task<VirtualAccount> Update(Guid accountId, VirtualAccount account);

        Task<VirtualAccount> UpdateBalance(Guid accountId, Guid virtualAccountId, decimal balance);

        Task Delete(Guid accountId, Guid virtualAccountId);
    }
}
