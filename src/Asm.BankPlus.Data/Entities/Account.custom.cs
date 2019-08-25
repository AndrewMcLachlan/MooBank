using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Account
    {
        public static implicit operator Models.Account(Account account)
        {
            return new Models.Account
            {
                Id = account.AccountId,
                Name = account.Name,
                AvailableBalance = account.AvailableBalance,
                CurrentBalance = account.AccountBalance,
                BalanceDate = account.LastUpdated,
            };
        }
    }
}
