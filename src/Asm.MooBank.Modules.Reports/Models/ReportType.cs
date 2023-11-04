using System.Linq.Expressions;

namespace Asm.MooBank.Models.Reports;

public struct ReportType
{
    private readonly ReportTypeInner _reportType;

    private ReportType(ReportTypeInner reportType) => _reportType = reportType;

    public const ReportTypeInner Income = ReportTypeInner.Income;
    public const ReportTypeInner Expenses = ReportTypeInner.Expenses;

    public static bool TryParse(string reportType, out ReportType value)
    {
        value = new ReportType(Income);
        if (String.IsNullOrWhiteSpace(reportType))
        {
            return false;
        }

        if (reportType.Equals("income", StringComparison.OrdinalIgnoreCase))
        {
            value = new ReportType(Income);
            return true;
        }

        if (reportType.Equals("expenses", StringComparison.OrdinalIgnoreCase))
        {
            value = new ReportType(Expenses);
            return true;
        }

        return false;
    }

    public static implicit operator ReportTypeInner(ReportType reportType) => reportType._reportType;
    public static implicit operator ReportType(ReportTypeInner reportType) => reportType switch
    {
        ReportTypeInner.Income => Income,
        ReportTypeInner.Expenses => Expenses,
        _ => throw new ArgumentOutOfRangeException(nameof(reportType))
    };
}

public enum ReportTypeInner
{
    Income = 0,
    Expenses = 1,

}

public static class ReportTypeExtensions
{
    public static Expression<Func<Domain.Entities.Transactions.Transaction, bool>> ToTransactionFilterExpression(this ReportType reportType) =>
        (ReportTypeInner)reportType switch
        {
            ReportType.Expenses => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Debit.Contains(t.TransactionType),
            ReportType.Income => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Credit.Contains(t.TransactionType),
            _ => (Domain.Entities.Transactions.Transaction t) => true
        };

    public static Func<Domain.Entities.Transactions.Transaction, bool> ToTransactionFilter(this ReportType reportType) =>
        (ReportTypeInner)reportType switch
        {
            ReportType.Expenses => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Debit.Contains(t.TransactionType),
            ReportType.Income => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Credit.Contains(t.TransactionType),
            _ => (Domain.Entities.Transactions.Transaction t) => true
        };

    public static IQueryable<Domain.Entities.Transactions.Transaction> WhereByReportType(this IQueryable<Domain.Entities.Transactions.Transaction> transactions, ReportType reportType) =>
        transactions.Where(reportType.ToTransactionFilterExpression());

    public static IEnumerable<Domain.Entities.Transactions.Transaction> WhereByReportType(this IEnumerable<Domain.Entities.Transactions.Transaction> transactions, ReportType reportType) =>
        transactions.Where(reportType.ToTransactionFilter());
}
