using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class AccountHolder
    {
        private ManyToManyCollection<AccountAccountHolder, Account, Guid> _accounts;

        public AccountHolder()
        {
            AccountLinks = new HashSet<AccountAccountHolder>();

            _accounts = new ManyToManyCollection<AccountAccountHolder, Account, Guid>(
                AccountLinks,
                (t) => new AccountAccountHolder { AccountId = this.AccountHolderId, AccountHolderId = t.AccountId },
                (t) => t.Account,
                (t) => t.AccountId,
                (t) => t.AccountId
            );
        }

        [NotMapped]
        public ICollection<Account> Accounts
        {
            get { return _accounts; }
            set
            {
                _accounts.Clear();
                _accounts.AddRange(value);
            }

        }

        public static explicit operator AccountHolder(Models.AccountHolder accountHolder)
        {
            return new AccountHolder
            {
                AccountHolderId = accountHolder.Id,
                EmailAddress = accountHolder.EmailAddress,
                FirstName = accountHolder.FirstName,
                LastName = accountHolder.LastName,
            };
        }

        public static explicit operator Models.AccountHolder(AccountHolder accountHolder)
        {
            return new Models.AccountHolder
            {
                Id = accountHolder.AccountHolderId,
                EmailAddress = accountHolder.EmailAddress,
                FirstName = accountHolder.FirstName,
                LastName = accountHolder.LastName,
            };
        }
    }
}
