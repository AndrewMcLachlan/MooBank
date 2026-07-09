using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Reports;

/// <summary>
/// A credit/debit total for a single account, returned by the set-based
/// <c>dbo.GetCreditDebitTotalsForAccounts</c> stored procedure.
/// </summary>
public class AccountCreditDebitTotal
{
    public Guid AccountId { get; set; }

    public TransactionFilterType TransactionType { get; set; }

    public decimal Total { get; set; }
}

/// <summary>
/// An end-of-month balance for a single account, returned by the set-based
/// <c>dbo.GetMonthlyBalancesForAccounts</c> stored procedure.
/// </summary>
public class AccountMonthlyBalance
{
    public Guid AccountId { get; set; }

    public required DateOnly PeriodEnd { get; set; }

    public required decimal Balance { get; set; }
}

/// <summary>
/// A monthly credit/debit total for a single account, returned by the set-based
/// <c>dbo.GetMonthlyCreditDebitTotalsForAccounts</c> stored procedure.
/// </summary>
public class AccountMonthlyCreditDebitTotal
{
    public Guid AccountId { get; set; }

    public DateOnly Month { get; set; }

    public TransactionFilterType TransactionType { get; set; }

    public decimal Total { get; set; }
}
