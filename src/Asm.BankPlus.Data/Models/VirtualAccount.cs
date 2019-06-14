using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Models
{
    public partial class VirtualAccount
    {
        public VirtualAccount()
        {
            RecurringTransactionDestinationVirtualAccount = new HashSet<RecurringTransaction>();
            RecurringTransactionSourceVirtualAccount = new HashSet<RecurringTransaction>();
            Transaction = new HashSet<Transaction>();
        }

        public Guid VirtualAccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public bool DefaultAccount { get; set; }
        public bool Closed { get; set; }

        public virtual ICollection<RecurringTransaction> RecurringTransactionDestinationVirtualAccount { get; set; }
        public virtual ICollection<RecurringTransaction> RecurringTransactionSourceVirtualAccount { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
