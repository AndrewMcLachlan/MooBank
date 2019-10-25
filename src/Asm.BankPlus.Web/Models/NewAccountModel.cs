using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Models
{
    public class NewAccountModel
    {
        public string Name { get; set; }

        public DateTime BalanceDate { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public AccountType AccountType { get; set; }

        public AccountController Controller { get; set; }

        public static explicit operator Account(NewAccountModel account)
        {
            return new Account
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
