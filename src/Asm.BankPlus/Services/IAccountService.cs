using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Services
{
    public interface IAccountService
    {
        Task<IEnumerable<Transaction>> RunTransactionRules(Guid accountId);
    }
}
