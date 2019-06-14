using System.Collections.Generic;
using Asm.BankPlus.Data.Models;

namespace Asm.BankPlus.Web.Models
{
    public class VirtualAccountsModel
    {
        public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }
    }
}