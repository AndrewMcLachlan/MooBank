using System.Collections.Generic;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTagTransactionTag
    {
        public TransactionTagTransactionTag()
        {
        }

        public int PrimaryTransactionTagId { get; set; }

        public int SecondaryTransactionTagId { get; set; }

        public virtual TransactionTag Primary { get; set; }

        public virtual TransactionTag Secondary { get; set; }
    }
}
