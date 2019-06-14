using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Models
{
    public partial class Transaction
    {
        public int TransactionId { get; set; }
        public Guid? TransactionGroupId { get; set; }
        public Guid VirtualAccountId { get; set; }
        public int TransactionTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionTime { get; set; }

        public virtual BankPlus.Models.TransactionType TransactionType { get; set; }
        public virtual VirtualAccount VirtualAccount { get; set; }
    }
}
