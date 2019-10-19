using System.Collections.Generic;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTag
    {
        public int TransactionTagId { get; set; }

        public string Name { get; set; }

        public bool Deleted { get; set; }

        public virtual ICollection<TransactionTagTransactionTag> TaggedToLinks { get; set; }

        public virtual ICollection<TransactionTagTransactionTag> TagLinks { get; set; }

        public virtual ICollection<TransactionTagRuleTransactionTag> TransactionTagRuleLinks { get; set; }
    }
}
