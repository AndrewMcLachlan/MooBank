using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutTrendReport : ReportQuery, IQuery<InOutTrendReport>
{
    public ReportInterval Interval { get; init; } = ReportInterval.Monthly;
}

internal class GetInOutTrendReportHandler(IQueryable<Domain.Entities.Transactions.Transaction> transactions) : IQueryHandler<GetInOutTrendReport, InOutTrendReport>
{
    public async ValueTask<InOutTrendReport> Handle(GetInOutTrendReport request, CancellationToken cancellationToken)
    {
        var groupedQuery = await transactions.Specify(new IncludeSplitsTagsAndOffsetsSpecification()).WhereByReportQuery(request).GroupBy(t => t.TransactionType).ToListAsync(cancellationToken);

        var income = GetTrendPoints(groupedQuery.Where(g => g.Key == TransactionType.Credit).SelectMany(g => g.AsQueryable()));
        var expenses = GetTrendPoints(groupedQuery.Where(g => g.Key == TransactionType.Debit).SelectMany(g => g.AsQueryable()));

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = income,
            Expenses = expenses,
        };
    }

    private static IEnumerable<TrendPoint> GetTrendPoints(IEnumerable<Domain.Entities.Transactions.Transaction> transactions)
    {
        return transactions.GroupBy(t => new DateOnly(t.TransactionTime.Year, t.TransactionTime.Month, 1)).OrderBy(g => g.Key).Select(g => new TrendPoint
        {
            Month = g.Key,
            GrossAmount = g.Sum(ReportedNetAmount)
        });
    }

    /// <summary>
    /// <see cref="Domain.Entities.Transactions.Transaction.NetAmount"/> less the splits whose tags
    /// are all excluded from reporting, so a transfer leaves the totals rather than inflating both
    /// sides. One reportable tag keeps the whole split in.
    /// </summary>
    /// <remarks>
    /// A transaction with no split left to report contributes nothing at all: its offsets go with
    /// it, rather than being deducted from a total the transaction is no longer part of.
    /// </remarks>
    private static decimal ReportedNetAmount(Domain.Entities.Transactions.Transaction transaction)
    {
        var reported = transaction.Splits
            .Where(s => !s.Tags.Any() || s.Tags.Any(tag => !tag.Settings.ExcludeFromReporting))
            .ToList();

        if (reported.Count == 0) return 0m;

        var sum = reported.Sum(s => s.GetNetAmount()) - transaction.OffsetFor.Sum(o => o.Amount);

        return transaction.TransactionType == TransactionType.Debit ? -sum : sum;
    }
}
