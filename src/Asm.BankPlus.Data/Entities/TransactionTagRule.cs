using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTagRule
    {
        public int TransactionTagRuleId { get; set; }

        public Guid AccountId { get; set; }

        public string Contains { get; set; }

        public virtual Account Account { get; set; }

        public virtual ICollection<TransactionTag> TransactionTags { get; set; }
    }
}
