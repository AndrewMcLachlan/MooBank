using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTagRule
    {
        public int TransactionTagRuleId { get; set; }

        public string Contains { get; set; }

        public int TransactionTagId { get; set; }

        public virtual TransactionTag TransactionTag { get; set; }
    }
}
