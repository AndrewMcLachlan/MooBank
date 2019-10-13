using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAccounts();

        Task<Account> GetAccount(Guid id);

        Task<Account> SetBalance(Guid id, decimal balance);

        Task<Account> SetBalance(Guid id, decimal balance, decimal availableBalance);
    }
}
