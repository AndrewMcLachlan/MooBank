using System.Collections.Generic;

namespace Asm.MooBank.Data.Entities
{
    public partial class TransactionTag
    {
        public int TransactionTagId { get; set; }

        public string Name { get; set; }

        public bool Deleted { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<TransactionTag> TaggedTo { get; set; }

        public virtual ICollection<TransactionTag> Tags { get; set; }

        public virtual ICollection<TransactionTagRule> Rules { get; set; }
    }
}
