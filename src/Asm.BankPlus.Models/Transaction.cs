using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = new Guid();
        public Guid? Reference { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionTime { get; set; }

        public TransactionType TransactionType { get; set; }

        public IEnumerable<TransactionTag> Tags { get; set; }

        public object ExtraInfo { get; set; }
    }
}
