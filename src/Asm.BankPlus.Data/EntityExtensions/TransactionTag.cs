using System;
using System.Collections.Generic;
using System.Linq;

namespace Asm.MooBank.Data.Entities
{
    public partial class TransactionTag : IEquatable<TransactionTag>
    {
        public TransactionTag()
        {
            TaggedTo = new HashSet<TransactionTag>();
            Tags = new HashSet<TransactionTag>();
            Rules = new HashSet<TransactionTagRule>();
        }

        public static implicit operator Models.TransactionTag(TransactionTag transactionTag)
        {
            if (transactionTag == null) return null;
            return new Models.TransactionTag()
            {
                Id = transactionTag.TransactionTagId,
                Name = transactionTag.Name,
                Tags = transactionTag.Tags.Where(t => t != null).Select(t => (Models.TransactionTag)t),
            };
        }

        public static implicit operator TransactionTag(Models.TransactionTag transactionTag)
        {
            return new TransactionTag
            {
                TransactionTagId = transactionTag.Id,
                Name = transactionTag.Name,
                Tags = transactionTag.Tags.Select(t => (TransactionTag)t).ToList(),
            };
        }

        #region Equals
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
            if ((object)null == (object)t1 ^ (object)null == (object)t2) return false;
            if ((object)null == t1 && (object)null == (object)t2) return true;

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
        #endregion
    }
}
