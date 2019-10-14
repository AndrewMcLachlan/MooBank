using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTagRule
    {
        public TransactionTagRule()
        {
            TransactionTagLinks = new HashSet<TransactionTagRuleTransactionTag>();
            TransactionTags = new ManyToManyCollection<TransactionTagRuleTransactionTag, TransactionTag, int>(
                TransactionTagLinks,
                (t) => new TransactionTagRuleTransactionTag { TransactionTagRuleId = this.TransactionTagRuleId, TransactionTagId = t.TransactionTagId },
                (t) => t.TransactionTag,
                (t) => t.TransactionTagId,
                (t) => t.TransactionTagId
            );
        }

        [NotMapped]
        public ICollection<TransactionTag> TransactionTags { get; set; }

        public static explicit operator Models.TransactionTagRule(TransactionTagRule rule)
        {
            return new Models.TransactionTagRule
            {
                Id = rule.TransactionTagRuleId,
                Contains = rule.Contains,
                Tags = rule.TransactionTags.Select(t => (Models.TransactionTag)t),
            };
        }

        public static explicit operator TransactionTagRule(Models.TransactionTagRule rule)
        {
            return new TransactionTagRule
            {
                TransactionTagRuleId = rule.Id,
                TransactionTags = rule.Tags.Select(t => (TransactionTag)t).ToList(),
            };
        }
    }
}
