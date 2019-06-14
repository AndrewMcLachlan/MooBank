using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Models
{
    public partial class TransactionType
    {
        public TransactionType()
        {
            Transaction = new HashSet<Transaction>();
        }

        public int TransactionTypeId { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
