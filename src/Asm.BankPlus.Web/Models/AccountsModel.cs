using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Models
{
    public class AccountsModel
    {
        public IEnumerable<MooBank.Models.Account> Accounts { get; set; }

        public decimal Position { get; set; }

        //public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }
    }
}