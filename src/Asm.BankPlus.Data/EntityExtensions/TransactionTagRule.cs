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
            TransactionTags = new HashSet<TransactionTag>();
        }

        public static explicit operator Models.TransactionTagRule(TransactionTagRule rule)
        {
            return new Models.TransactionTagRule
            {
                Id = rule.TransactionTagRuleId,
                Contains = rule.Contains,
                Tags = rule.TransactionTags.Where(t => t != null && !t.Deleted).Select(t => (Models.TransactionTag)t),
            };
        }

        public static explicit operator TransactionTagRule(Models.TransactionTagRule rule)
        {
            return new TransactionTagRule
            {
                TransactionTagRuleId = rule.Id,
                Contains = rule.Contains,
                TransactionTags = rule.Tags.Where(t=> t != null).Select(t => (TransactionTag)t).ToList(),
            };
        }
    }
}
