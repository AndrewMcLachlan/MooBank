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

    [Display(Name = "Visa")]
    Visa = 5,

    [Display(Name = "MasterCard")]
    MasterCard = 6,

    [Display(Name = "Direct Debit")]
    DirectDebit = 7,

    [Display(Name = "EFTPOS")]
    Eftpos = 8,

    [Display(Name = "ATM")]
    Atm = 9,

    [Display(Name = "OSKO")]
    Osko = 10,

    [Display(Name = "BPAY")]
    Bpay = 11,
}
