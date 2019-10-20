using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Asm.BankPlus.Data.Entities
{
    public partial class Transaction
    {
        public Transaction()
        {
            TransactionTagLinks = new HashSet<TransactionTransactionTag>();
            TransactionTags = new ManyToManyCollection<TransactionTransactionTag, TransactionTag, int>(
                TransactionTagLinks,
                (t) => new TransactionTransactionTag { TransactionId = this.TransactionId, TransactionTagId = t.TransactionTagId },
                (t) => t.TransactionTag,
                (t) => t.TransactionTagId,
                (t) => t.TransactionTagId
            );
        }

        [NotMapped]
        public ICollection<TransactionTag> TransactionTags { get; set; }

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
                Tags = transaction.TransactionTags.Select(t => (Models.TransactionTag)t),
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
