using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Models
{
    public class AccountBalance
    {
        public DateTime BalanceDate { get; set; }

        public string AccountName { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal AvailableBalance { get; set; }
    }
}
