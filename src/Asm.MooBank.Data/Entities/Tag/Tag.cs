using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.Tag;

[AggregateRoot]
public partial class Tag : IEquatable<Tag>
{
    public Tag()
    {
        Settings = new();
        TaggedTo = new HashSet<Tag>();
        Tags = new HashSet<Tag>();
        Transactions = new HashSet<Transaction>();
    }


    public int Id { get; set; }

    public string Name { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<Tag> TaggedTo { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }

    public virtual TransactionTagSettings Settings { get; set; }

    #region Equality
    public bool Equals(Tag? other)
    {
        if (other is null) return false;

        return other.Id == Id;
    }


    public override bool Equals(object? obj) => Equals(obj as Tag);

    public override int GetHashCode() => Id.GetHashCode();
    #endregion
}


public class TransactionTagEqualityComparer : IEqualityComparer<Tag>
{
    public bool Equals(Tag? x, Tag? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        return x.Id == y.Id;


    }

    public int GetHashCode([DisallowNull] Tag obj) => obj.Id.GetHashCode();
}