using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Asm.BankPlus.DataAccess;

namespace Asm.BankPlus.Models
{
    public class AccountModel
    {
        public IEnumerable<Account> Accounts { get; set; }

        public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }
    }
}