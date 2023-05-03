namespace Asm.MooBank.Models.Queries.Reports;

public abstract record ReportQuery
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }
}

public static class ReportQueryExtensions
{
    public static IQueryable<Domain.Entities.Transactions.Transaction> WhereByQuery(this IQueryable<Domain.Entities.Transactions.Transaction> transactions, ReportQuery query)
    {
        var start = query.Start.ToStartOfDay();
        var end = query.End.ToEndOfDay();

        return transactions.Where(t => t.AccountId == query.AccountId && !t.ExcludeFromReporting && t.TransactionTime >= start && t.TransactionTime <= end);
    }
}