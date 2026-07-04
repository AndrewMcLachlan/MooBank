namespace Asm.MooBank.Models;

public static class AccountTypeReports
{
    private static readonly IReadOnlyList<ReportKind> Transaction =
    [
        ReportKind.InOut,
        ReportKind.TopTags,
        ReportKind.Breakdown,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.MonthlyBalances,
    ];

    private static readonly IReadOnlyList<ReportKind> Savings =
    [
        ReportKind.MonthlyBalances,
        ReportKind.SavingsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.InOut,
    ];

    private static readonly IReadOnlyList<ReportKind> Credit =
    [
        ReportKind.InOut,
        ReportKind.TopTags,
        ReportKind.Breakdown,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.MonthlyBalances,
    ];

    private static readonly IReadOnlyList<ReportKind> Mortgage =
    [
        ReportKind.MonthlyBalances,
        ReportKind.PrincipalVsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> Superannuation =
    [
        ReportKind.MonthlyBalances,
        ReportKind.SuperContributions,
        ReportKind.SuperReturns,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> Investment =
    [
        ReportKind.MonthlyBalances,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> Loan =
    [
        ReportKind.MonthlyBalances,
        ReportKind.PrincipalVsInterest,
        ReportKind.TagTrend,
        ReportKind.AllTags,
    ];

    private static readonly IReadOnlyList<ReportKind> Broker =
    [
        ReportKind.MonthlyBalances,
        ReportKind.TagTrend,
        ReportKind.AllTags,
        ReportKind.TopTags,
        ReportKind.Breakdown,
    ];

    public static IReadOnlyList<ReportKind> For(AccountType accountType) => accountType switch
    {
        AccountType.Transaction => Transaction,
        AccountType.Savings => Savings,
        AccountType.Credit => Credit,
        AccountType.Mortgage => Mortgage,
        AccountType.Superannuation => Superannuation,
        AccountType.Investment => Investment,
        AccountType.Loan => Loan,
        AccountType.Broker => Broker,
        _ => Transaction,
    };
}
