using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.Transactions;

public class TransactionComparer : IEqualityComparer<Transaction>
{
    public bool Equals(Transaction? x, Transaction? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.TransactionTime == y.TransactionTime
            && x.Amount == y.Amount;
    }

    public int GetHashCode([DisallowNull] Transaction obj) =>
        obj.TransactionTime.GetHashCode() ^ obj.Amount.GetHashCode();
}
