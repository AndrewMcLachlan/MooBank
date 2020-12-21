using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Account
    {
        public Account()
        {
            Transaction = new HashSet<Transaction>();
            AccountHolders = new HashSet<AccountHolder>();
        }

        public static implicit operator Models.Account(Account account)
        {
            return new Models.Account
            {
                Id = account.AccountId,
                Name = account.Name,
                AvailableBalance = account.AvailableBalance,
                CurrentBalance = account.AccountBalance,
                IncludeInPosition = account.IncludeInPosition,
                BalanceDate = account.LastUpdated,
                AccountType = account.AccountType,
                Controller = account.AccountController,
            };
        }

        public static explicit operator Account(Models.Account account)
        {
            return new Account
            {
                AccountId = account.Id,
                Name = account.Name,
                AvailableBalance = account.AvailableBalance,
                AccountBalance = account.CurrentBalance,
                IncludeInPosition = account.IncludeInPosition,
                LastUpdated = account.BalanceDate,
                AccountType = account.AccountType,
                AccountController = account.Controller,
            };
        }
    }
}
