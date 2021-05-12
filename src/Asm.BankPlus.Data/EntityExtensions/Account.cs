using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Account
    {
        public Account()
        {
            Transactions = new HashSet<Transaction>();
            AccountHolders = new HashSet<AccountHolder>();
        }

        public static implicit operator Models.Account(Account account)
        {
            return new Models.Account
            {
                Id = account.AccountId,
                Name = account.Name,
                Description = account.Description,
                AvailableBalance = account.AvailableBalance,
                CurrentBalance = account.AccountBalance,
                IncludeInPosition = account.IncludeInPosition,
                BalanceDate = account.LastUpdated,
                AccountType = account.AccountType,
                Controller = account.AccountController,
                ImporterTypeId = account.ImportAccount?.ImporterTypeId,
            };
        }

        public static explicit operator Account(Models.Account account)
        {
            return new Account
            {
                AccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
                Name = account.Name,
                Description = account.Description,
                AvailableBalance = account.AvailableBalance,
                AccountBalance = account.CurrentBalance,
                IncludeInPosition = account.IncludeInPosition,
                LastUpdated = account.BalanceDate,
                AccountType = account.AccountType,
                AccountController = account.Controller,
                ImportAccount = account.ImporterTypeId == null ? null : new ImportAccount { ImporterTypeId = account.ImporterTypeId.Value, AccountId = account.Id },
            };
        }
    }
}
