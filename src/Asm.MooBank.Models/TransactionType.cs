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
