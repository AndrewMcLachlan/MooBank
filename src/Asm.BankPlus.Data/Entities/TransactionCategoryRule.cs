using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionCategoryRule
    {
        public int TransactionCategoryRuleId { get; set; }

        public string Contains { get; set; }

        public int TransactionCategoryId { get; set; }

        public virtual TransactionCategory TransactionCategory { get; set; }
    }
}
