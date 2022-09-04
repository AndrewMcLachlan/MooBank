using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Data.Entities
{
    public class TransactionTagRuleTransactionTag
    {
        public int TransactionTagRuleId { get; set; }

        public int TransactionTagId { get; set; }

        public virtual TransactionTagRule TransactionTagRule { get; set; }

        public virtual TransactionTag TransactionTag { get; set; }
    }
}
