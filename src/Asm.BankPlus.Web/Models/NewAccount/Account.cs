using System;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Models.NewAccount
{
    public class Account
    {
        public string Name { get; set; }

        public DateTime BalanceDate { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public AccountType AccountType { get; set; }

        public AccountController Controller { get; set; }

        public static explicit operator BankPlus.Models.Account(Account account)
        {
            return new BankPlus.Models.Account
            {
                Id = Guid.NewGuid(),
                AccountType = account.AccountType,
                AvailableBalance = account.AvailableBalance,
                BalanceDate = account.BalanceDate,
                Controller = account.Controller,
                CurrentBalance = account.CurrentBalance,
                Name = account.Name,
            };
        }
    }
}
