namespace Asm.MooBank.Models;

public static class AccountTypeReports
{
    private static readonly IReadOnlyList<ReportKind> _transaction =
    [
        ReportKind.InOut,
        ReportKind.TopTags,
        ReportKind.Breakdown,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.MonthlyBalances,
    ];

    private static readonly IReadOnlyList<ReportKind> _savings =
    [
        ReportKind.MonthlyBalances,
        ReportKind.SavingsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.InOut,
    ];

    private static readonly IReadOnlyList<ReportKind> _credit =
    [
        ReportKind.InOut,
        ReportKind.TopTags,
        ReportKind.Breakdown,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.MonthlyBalances,
    ];

    private static readonly IReadOnlyList<ReportKind> _mortgage =
    [
        ReportKind.MonthlyBalances,
        ReportKind.PrincipalVsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> _superannuation =
    [
        ReportKind.MonthlyBalances,
        ReportKind.SuperContributions,
        ReportKind.SuperReturns,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> _investment =
    [
        ReportKind.MonthlyBalances,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> _loan =
    [
        ReportKind.MonthlyBalances,
        ReportKind.PrincipalVsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> _broker =
    [
        ReportKind.MonthlyBalances,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.TopTags,
        ReportKind.Breakdown,
    ];

    public static IReadOnlyList<ReportKind> For(AccountType accountType) => accountType switch
    {
        AccountType.Transaction => _transaction,
        AccountType.Savings => _savings,
        AccountType.Credit => _credit,
        AccountType.Mortgage => _mortgage,
        AccountType.Superannuation => _superannuation,
        AccountType.Investment => _investment,
        AccountType.Loan => _loan,
        AccountType.Broker => _broker,
        _ => _transaction,
    };
}
