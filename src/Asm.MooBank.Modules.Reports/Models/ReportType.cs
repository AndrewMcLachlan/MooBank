using System.Linq.Expressions;

namespace Asm.MooBank.Models.Reports;

public enum ReportType
{
    Income = 0,
    Expenses = 1,
}

public static class ReportTypeExtensions
{
    public static Expression<Func<Domain.Entities.Transactions.Transaction, bool>> ToTransactionFilterExpression(this ReportType reportType) =>
        reportType switch
        {
            ReportType.Expenses => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Debit.Contains(t.TransactionType),
            ReportType.Income => (Domain.Entities.Transactions.Transaction t) => TransactionTypes.Credit.Contains(t.TransactionType),
            _ => (Domain.Entities.Transactions.Transaction t) => true
        };

    public static Func<Domain.Entities.Transactions.Transaction, bool> ToTransactionFilter(this ReportType reportType) =>
        reportType switch
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
