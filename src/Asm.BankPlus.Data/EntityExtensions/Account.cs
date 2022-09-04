using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Data.Entities
{
    public partial class Account
    {
        public Account()
        {
            VirtualAccounts = new HashSet<VirtualAccount>();
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
                VirtualAccounts = account.VirtualAccounts != null && account.VirtualAccounts.Any() ?
                                  account.VirtualAccounts.Select(v => (Models.VirtualAccount)v)
                                                         .Union(new[] { new Models.VirtualAccount { Id = Guid.Empty, Name = "Remaining", Balance = account.AccountBalance - account.VirtualAccounts.Sum(v => v.Balance) } }).ToArray() :
                                    Array.Empty<Models.VirtualAccount>(),
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
