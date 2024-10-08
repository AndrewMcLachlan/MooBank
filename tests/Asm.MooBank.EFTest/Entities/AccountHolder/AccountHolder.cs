﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.AccountHolder;

public partial class AccountHolder
{
    public AccountHolder()
    {
        Accounts = new HashSet<Account.Account>();
    }
    public Guid AccountHolderId { get; set; }
    public string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [NotMapped]
    public virtual ICollection<Account.Account> Accounts { get; set; }
}
