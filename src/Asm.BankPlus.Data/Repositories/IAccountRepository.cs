using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Data.Repositories
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAccounts();

        Task<Account> GetAccount(Guid id);

        Task<Account> SetBalance(Guid id, decimal balance);

        Task<Account> SetBalance(Guid id, decimal? balance, decimal? availableBalance);

        Task<Account> Create(Account account);

        Task<Account> CreateImportAccount(Account account, int importerTypeId);

        Task<Account> Update(Account account);

        Task<decimal> GetPosition();

    }
}
