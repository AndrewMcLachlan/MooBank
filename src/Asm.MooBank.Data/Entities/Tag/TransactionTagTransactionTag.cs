using System.Collections.Generic;

namespace Asm.MooBank.Domain.Entities.Tag
{
    public partial class TransactionTagTransactionTag
    {
        public TransactionTagTransactionTag()
        {
        }

        public int PrimaryTransactionTagId { get; set; }

        public int SecondaryTransactionTagId { get; set; }

        public virtual Tag Primary { get; set; }

        public virtual Tag Secondary { get; set; }
    }
}
