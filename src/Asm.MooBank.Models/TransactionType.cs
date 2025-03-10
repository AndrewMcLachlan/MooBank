﻿using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Models;

public enum TransactionType
{
    [Display(Name = "Not Set")]
    NotSet = 0,

    [Display(Name = "Credit")]
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

public static class TransactionTypes
{
    public static readonly IEnumerable<TransactionType> Credit = new[] { TransactionType.Credit, TransactionType.RecurringCredit, TransactionType.BalanceAdjustmentCredit };

    public static readonly IEnumerable<TransactionType> Debit = new[] { TransactionType.Debit, TransactionType.RecurringDebit, TransactionType.BalanceAdjustmentDebit };

    public static bool IsCredit(this TransactionType type) => Credit.Contains(type);

    public static bool IsDebit(this TransactionType type) => Debit.Contains(type);
}
