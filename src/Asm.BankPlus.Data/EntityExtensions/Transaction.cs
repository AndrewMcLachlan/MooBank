using System.Collections.Generic;
using System.Linq;

namespace Asm.MooBank.Data.Entities
{
    public partial class Transaction
    {
        public Transaction()
        {
            TransactionTags = new HashSet<TransactionTag>();
        }

        public static explicit operator Models.Transaction(Transaction transaction)
        {
            return new Models.Transaction
            {
                Id = transaction.TransactionId,
                Reference = transaction.TransactionReference,
                Amount = transaction.Amount,
                TransactionTime = transaction.TransactionTime,
                TransactionType = transaction.TransactionType,
                AccountId = transaction.AccountId,
                Description = transaction.Description,
                Tags = transaction.TransactionTags.Where(t => !t.Deleted).Select(t => (Models.TransactionTag)t),
            };
        }

        public static explicit operator Transaction(Models.Transaction transaction)
        {
            return new Transaction
            {
                //TransactionId = transaction.Id == Guid.Empty ? Guid.NewGuid() : transaction.Id,
                TransactionId = transaction.Id,
                TransactionReference = transaction.Reference,
                Amount = transaction.Amount,
                TransactionTime = transaction.TransactionTime,
                TransactionType = transaction.TransactionType,
                AccountId = transaction.AccountId,
                Description = transaction.Description,
            };
        }
    }
}
