using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Transaction
    {
        public Guid TransactionId { get; set; }

        public Guid? TransactionReference { get; set; }

        public Guid AccountId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTime TransactionTime { get; set; }

        public ICollection<TransactionTag> TransactionTags { get; set; }

        public virtual Account Account { get; set; }

        public BankPlus.Models.TransactionType TransactionType { get; set; }
    }
}
