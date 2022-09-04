using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Models
{
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

        [Display(Name = "Balance Adjustment")]
        BalanceAdjustment = 5,
    }
}
