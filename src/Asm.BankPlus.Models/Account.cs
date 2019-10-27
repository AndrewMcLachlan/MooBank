using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset BalanceDate { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public bool IncludeInPosition { get; set; }

        public AccountType AccountType { get; set; }

        public AccountController Controller { get; set; }
    }
}
