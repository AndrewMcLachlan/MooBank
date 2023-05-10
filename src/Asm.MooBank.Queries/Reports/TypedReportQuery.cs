using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public abstract record TypedReportQuery : ReportQuery
{
    public required ReportType ReportType { get; init; }
}

public static class TypedReportQueryExtensions
{
    public static IQueryable<Domain.Entities.Transactions.Transaction> WhereByReportQuery(this IQueryable<Domain.Entities.Transactions.Transaction> transactions, TypedReportQuery query) =>
        transactions.WhereByReportQuery(query as ReportQuery).WhereByReportType(query.ReportType);
}