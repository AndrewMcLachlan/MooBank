//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Asm.BankPlus.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class VirtualAccount
    {
        public VirtualAccount()
        {
            this.DestinationRecurringTransactions = new HashSet<RecurringTransaction>();
            this.SourceRecurringTransactions = new HashSet<RecurringTransaction>();
            this.Transactions = new HashSet<Transaction>();
        }
    
        public System.Guid VirtualAccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public bool DefaultAccount { get; set; }
        public bool Closed { get; set; }
    
        public virtual ICollection<RecurringTransaction> DestinationRecurringTransactions { get; set; }
        public virtual ICollection<RecurringTransaction> SourceRecurringTransactions { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
