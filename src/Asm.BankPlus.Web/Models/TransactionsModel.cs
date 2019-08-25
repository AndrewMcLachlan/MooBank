using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Models
{
    public class TransactionsModel
    {
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}
