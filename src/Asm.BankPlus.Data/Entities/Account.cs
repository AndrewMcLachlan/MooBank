﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Account
    {
        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal AccountBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public bool IncludeInPosition { get; set; }

        public bool UpdateVirtualAccount { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        [Column("AccountControllerId")]
        public Models.AccountController AccountController { get; set; }

        [Column("AccountTypeId")]
        public Models.AccountType AccountType { get; set; }

        public virtual ICollection<Transaction> Transaction { get; set; }

        public virtual ICollection<AccountAccountHolder> AccountHolderLinks { get; set; }
    }
}
