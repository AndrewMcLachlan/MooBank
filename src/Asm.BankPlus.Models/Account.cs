using System;
using System.Collections.Generic;
using System.Linq;

namespace Asm.BankPlus.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset BalanceDate { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public bool IncludeInPosition { get; set; }

        public AccountType AccountType { get; set; }

        public AccountController Controller { get; set; }

        public int? ImporterTypeId { get; set; }

        public IEnumerable<VirtualAccount> VirtualAccounts { get; set; }

        public decimal VirtualAccountRemainingBalance
        {
            get => CurrentBalance - (VirtualAccounts?.Sum(v => v.Balance) ?? 0);
        }
    }
}
