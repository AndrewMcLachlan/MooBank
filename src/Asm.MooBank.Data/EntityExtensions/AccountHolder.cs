using System.Collections.Generic;

namespace Asm.MooBank.Data.Entities
{
    public partial class AccountHolder
    {
        public AccountHolder()
        {
            Accounts = new HashSet<Account>();
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
