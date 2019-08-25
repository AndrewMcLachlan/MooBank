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
        }

        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public bool UpdateVirtualAccount { get; set; }
        public DateTime LastUpdated { get; set; }

        [Column("AccountTypeId")]
        public BankPlus.Models.AccountType AccountType { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
