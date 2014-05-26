using Asm.BankPlus.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Asm.BankPlus.Web.Models
{
    public class VirtualAccountsModel
    {
        public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }
    }
}