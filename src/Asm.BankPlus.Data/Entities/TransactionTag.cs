﻿using System.Collections.Generic;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTag
    {
        public TransactionTag()
        {
            TaggedToLink = new HashSet<TransactionTagTransactionTag>();
            TagsLink = new HashSet<TransactionTagTransactionTag>();
            TransactionTagRules = new HashSet<TransactionTagRule>();
        }

        public int TransactionTagId { get; set; }

        public string Name { get; set; }

        public bool Deleted { get; set; }

        public virtual ICollection<TransactionTagTransactionTag> TaggedToLink { get; set; }

        public virtual ICollection<TransactionTagTransactionTag> TagsLink { get; set; }

        public virtual ICollection<TransactionTagRule> TransactionTagRules { get; set; }
    }
}
