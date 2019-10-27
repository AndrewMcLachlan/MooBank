using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Models
{
    public class AccountsModel
    {
        public IEnumerable<Account> Accounts { get; set; }

        public decimal Position { get; set; }

        //public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }
    }
}