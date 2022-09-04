using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Data.Entities
{
    public partial class VirtualAccount
    {
        public VirtualAccount()
        {
            //RecurringTransactionDestinationVirtualAccount = new HashSet<RecurringTransaction>();
            //RecurringTransactionSourceVirtualAccount = new HashSet<RecurringTransaction>();
        }

        public Guid VirtualAccountId { get; set; }
        public Guid AccountId { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Balance { get; set; }

        public virtual Account Account { get; set; }


        //        public virtual ICollection<RecurringTransaction> RecurringTransactionDestinationVirtualAccount { get; set; }
        //public virtual ICollection<RecurringTransaction> RecurringTransactionSourceVirtualAccount { get; set; }
    }
}
