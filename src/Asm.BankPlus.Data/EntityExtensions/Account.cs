using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Account
    {
        private ManyToManyCollection<AccountAccountHolder, AccountHolder, Guid> _accountHolders;

        public Account()
        {
            Transaction = new HashSet<Transaction>();
            AccountHolderLinks = new HashSet<AccountAccountHolder>();

            _accountHolders = new ManyToManyCollection<AccountAccountHolder, AccountHolder, Guid>(
                AccountHolderLinks,
                (t) => new AccountAccountHolder { AccountId = this.AccountId, AccountHolderId = t.AccountHolderId },
                (t) => t.AccountHolder,
                (t) => t.AccountHolderId,
                (t) => t.AccountHolderId
            );
        }

        [NotMapped]
        public ICollection<AccountHolder> AccountHolders
        {
            get { return _accountHolders; }
            set
            {
                _accountHolders.Clear();
                _accountHolders.AddRange(value);
            }

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
