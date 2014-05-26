using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus
{
    public interface IAccountScraper
    {
        IEnumerable<AccountBalance> GetAccountBalances();
    }
}
