using System.Linq.Expressions;
using Asm.MooBank.Models;

namespace Asm.MooBank;

public static class TransactionTypes
{
    public static readonly IEnumerable<TransactionType> Credit = new[] { TransactionType.Credit, TransactionType.RecurringCredit, TransactionType.BalanceAdjustmentCredit };

    public static readonly IEnumerable<TransactionType> Debit = new[] { TransactionType.Debit, TransactionType.RecurringDebit, TransactionType.BalanceAdjustmentDebit };

    public static bool IsCredit(this TransactionType type) => Credit.Contains(type);

    public static bool IsDebit(this TransactionType type) => Debit.Contains(type);
}
