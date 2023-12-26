namespace Asm.MooBank.Modules.Reports.Queries;

public abstract record ReportQuery
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }
}

public static class ReportQueryExtensions
{
    public static IQueryable<Domain.Entities.Transactions.Transaction> WhereByReportQuery(this IQueryable<Domain.Entities.Transactions.Transaction> transactions, ReportQuery query)
    {
        var start = query.Start.ToStartOfDay();
        var end = query.End.ToEndOfDay();

        return transactions.Where(t => t.AccountId == query.AccountId && !t.ExcludeFromReporting && t.TransactionTime >= start && t.TransactionTime <= end);
    }

    public static IQueryable<Domain.Entities.Transactions.Transaction> ExcludeOffset(this IQueryable<Domain.Entities.Transactions.Transaction> transactions) =>
        transactions.Where(t => t.OffsetFor.Count == 0);
}
