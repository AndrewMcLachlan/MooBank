using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.TransactionTags;

public partial class TransactionTag
{
    public TransactionTag()
    {
        TaggedTo = new HashSet<TransactionTag>();
        Tags = new HashSet<TransactionTag>();
        //Rules = new HashSet<TransactionTagRule>();
    }


    public int TransactionTagId { get; set; }

    public string Name { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<TransactionTag> TaggedTo { get; set; }

    public virtual ICollection<TransactionTag> Tags { get; set; }

    //public virtual ICollection<TransactionTagRule> Rules { get; set; }
}


public class TransactionTagEqualityComparer : IEqualityComparer<TransactionTag>
{
    public bool Equals(TransactionTag? x, TransactionTag? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        return x.TransactionTagId == y.TransactionTagId;


    }

    public int GetHashCode([DisallowNull] TransactionTag obj) => obj.TransactionTagId.GetHashCode();
}