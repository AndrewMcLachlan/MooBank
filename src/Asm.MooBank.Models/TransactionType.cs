using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Models;

public enum TransactionType
{
    [Display(Name = "Not Set")]
    NotSet = 0,

    [Display(Name = "Credit")]
    Credit = 1,

    [Display(Name = "Debit")]
    Debit = 2,
}

public static class TransactionTypes
{
    public static readonly IEnumerable<TransactionType> Credit = [TransactionType.Credit];

    public static readonly IEnumerable<TransactionType> Debit = [TransactionType.Debit];

    public static bool IsCredit(this TransactionType type) => Credit.Contains(type);

    public static bool IsDebit(this TransactionType type) => Debit.Contains(type);
}
