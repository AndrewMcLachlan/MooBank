using System;
using System.Collections.Generic;

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

        public int TransactionCategoryId { get; set; }

        public virtual TransactionCategory TransactionCategory { get; set; }

        public virtual Account Account { get; set; }
        public BankPlus.Models.TransactionType TransactionType { get; set; }
    }
}
