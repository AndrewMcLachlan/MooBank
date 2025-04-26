using System.Linq.Expressions;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Reports.Models;

public struct ReportType
{
    private readonly TransactionFilterType _reportType;

    private ReportType(TransactionFilterType reportType) => _reportType = reportType;

    public const TransactionFilterType Credit = TransactionFilterType.Credit;
    public const TransactionFilterType Debit = TransactionFilterType.Debit;

    public static bool TryParse(string reportType, out ReportType value)
    {
        value = new ReportType(Credit);
        if (String.IsNullOrWhiteSpace(reportType))
        {
            return false;
        }

        if (reportType.Equals("credit", StringComparison.OrdinalIgnoreCase))
        {
            value = new ReportType(Credit);
            return true;
        }

        if (reportType.Equals("debit", StringComparison.OrdinalIgnoreCase))
        {
            value = new ReportType(Debit);
            return true;
        }

        return false;
    }

    public static implicit operator TransactionFilterType(ReportType reportType) => reportType._reportType;
    public static implicit operator ReportType(TransactionFilterType reportType) => reportType switch
    {
        TransactionFilterType.Credit => Credit,
        TransactionFilterType.Debit => Debit,
        _ => throw new ArgumentOutOfRangeException(nameof(reportType))
    };
}


public static class ReportTypeExtensions
{
    public static Expression<Func<Domain.Entities.Transactions.Transaction, bool>> ToTransactionFilterExpression(this ReportType reportType) =>
        (TransactionFilterType)reportType switch
        {
            ReportType.Debit => (t) => t.TransactionType == TransactionType.Debit,
            ReportType.Credit => (t) => t.TransactionType == TransactionType.Credit,
            _ => (t) => true
        };

    public static Func<Domain.Entities.Transactions.Transaction, bool> ToTransactionFilter(this ReportType reportType) =>
        (TransactionFilterType)reportType switch
        {
            ReportType.Debit => (t) => t.TransactionType == TransactionType.Debit,
            ReportType.Credit => (t) => t.TransactionType == TransactionType.Credit,
            _ => (t) => true
        };

    public static IQueryable<Domain.Entities.Transactions.Transaction> WhereByReportType(this IQueryable<Domain.Entities.Transactions.Transaction> transactions, ReportType reportType) =>
        transactions.Where(reportType.ToTransactionFilterExpression());

    public static IEnumerable<Domain.Entities.Transactions.Transaction> WhereByReportType(this IEnumerable<Domain.Entities.Transactions.Transaction> transactions, ReportType reportType) =>
        transactions.Where(reportType.ToTransactionFilter());
}
