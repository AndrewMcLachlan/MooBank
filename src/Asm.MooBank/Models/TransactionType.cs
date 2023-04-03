using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Models;

public enum TransactionType
{
    [Display(Name="Credit")]
    Credit = 1,

    [Display(Name = "Debit")]
    Debit = 2,

    [Display(Name = "Recurring Credit")]
    RecurringCredit = 3,

    [Display(Name = "Recurring Debit")]
    RecurringDebit = 4,

    [Display(Name = "Balance Adjustment Credit")]
    BalanceAdjustmentCredit = 5,

    [Display(Name = "Balance Adjustment Debit")]
    BalanceAdjustmentDebit = 6,
}
