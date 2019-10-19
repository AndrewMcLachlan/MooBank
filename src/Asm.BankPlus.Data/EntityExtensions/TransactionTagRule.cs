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
        private ManyToManyCollection<TransactionTagRuleTransactionTag, TransactionTag, int> _transactionTags;

        public TransactionTagRule()
        {
            TransactionTagLinks = new HashSet<TransactionTagRuleTransactionTag>();
            _transactionTags = new ManyToManyCollection<TransactionTagRuleTransactionTag, TransactionTag, int>(
                TransactionTagLinks,
                manyEntityAdd: (t) => new TransactionTagRuleTransactionTag { TransactionTagRuleId = this.TransactionTagRuleId, TransactionTagId = t.TransactionTagId },
                childSelector: (t) => t.TransactionTag,
                manyEntityChildKeySelector: (t) => t.TransactionTagId,
                childKeySelector: (t) => t.TransactionTagId
            );
        }

        [NotMapped]
        public ICollection<TransactionTag> TransactionTags
        {
            get { return _transactionTags;  }
            set
            {
                _transactionTags.Clear();
                _transactionTags.AddRange(value);
            }

        }

        public static explicit operator Models.TransactionTagRule(TransactionTagRule rule)
        {
            return new Models.TransactionTagRule
            {
                Id = rule.TransactionTagRuleId,
                Contains = rule.Contains,
                Tags = rule.TransactionTags.Where(t => t != null).Select(t => (Models.TransactionTag)t),
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
