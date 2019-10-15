using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTag : IEquatable<TransactionTag>
    {
        public TransactionTag()
        {
            TaggedToLink = new HashSet<TransactionTagTransactionTag>();
            TagsLink = new HashSet<TransactionTagTransactionTag>();
            TransactionTagRules = new HashSet<TransactionTagRule>();

            Tags = new ManyToManyCollection<TransactionTagTransactionTag, TransactionTag, int>(
                TagsLink,
                (t) => new TransactionTagTransactionTag { PrimaryTransactionTagId = this.TransactionTagId, SecondaryTransactionTagId = t.TransactionTagId },
                (t) => t.Secondary,
                (t) => t.SecondaryTransactionTagId,
                (t) => t.TransactionTagId
            );
        }

        [NotMapped]
        public ICollection<TransactionTag> Tags { get; set; }


        public static implicit operator Models.TransactionTag(TransactionTag transactionTag)
        {
            return new Models.TransactionTag()
            {
                Id = transactionTag.TransactionTagId,
                Name = transactionTag.Name,
            };
        }

        public static implicit operator TransactionTag(Models.TransactionTag transactionTag)
        {
            return new TransactionTag
            {
                TransactionTagId = transactionTag.Id,
                Name = transactionTag.Name,
            };
        }

        public bool Equals(TransactionTag other)
        {
            if (other == null) return false;

            bool result = this.TransactionTagId == other.TransactionTagId;

            if (result && this.Name != other.Name) throw new InvalidOperationException("Two tags with matching IDs cannot have different names");

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is TransactionTag tag)) return false;

            return Equals(tag);
        }

        public static bool operator ==(TransactionTag t1, TransactionTag t2)
        {
            if (t1 == null ^ t2 == null) return false;
            if (t1 == null && t2 == null) return true;

            return t1.Equals(t2);
        }

        public static bool operator !=(TransactionTag t1, TransactionTag t2)
        {
            return !(t1 == t2);
        }

        public override int GetHashCode()
        {
            return this.TransactionTagId.GetHashCode() ^ TransactionTagId.GetHashCode();
        }
    }
}
