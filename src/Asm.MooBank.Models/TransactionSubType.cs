using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Models;

public enum TransactionSubType
{
    [Display(Name = "Opening Balance")]
    OpeningBalance = 1,

    [Display(Name = "Recurring")]
    Recurring = 2,

    [Display(Name = "Balance Adjustment")]
    BalanceAdjustment = 3,

    [Display(Name = "Transfer")]
    Transfer = 4,
}
