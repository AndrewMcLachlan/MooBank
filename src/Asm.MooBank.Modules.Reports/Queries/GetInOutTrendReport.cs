using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutTrendReport : ReportQuery, IQuery<InOutTrendReport>;

internal class GetInOutTrendReportHandler(IQueryable<Transaction> transactions, ISecurity securityRepository) : IQueryHandler<GetInOutTrendReport, InOutTrendReport>
{
    public async ValueTask<InOutTrendReport> Handle(GetInOutTrendReport request, CancellationToken cancellationToken)
    {
        securityRepository.AssertInstrumentPermission(request.AccountId);

        var groupedQuery = await transactions.WhereByReportQuery(request).GroupBy(t => t.TransactionType).ToListAsync(cancellationToken);

        var income = GetTrendPoints(groupedQuery.Where(g => g.Key.IsCredit()).SelectMany(g => g.AsQueryable()));
        var expenses = GetTrendPoints(groupedQuery.Where(g => g.Key.IsDebit()).SelectMany(g => g.AsQueryable()));

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = income,
            Expenses = expenses,
        };
    }

    private static IEnumerable<TrendPoint> GetTrendPoints(IEnumerable<Transaction> transactions)
    {
        return transactions.GroupBy(t => new DateOnly(t.TransactionTime.Year, t.TransactionTime.Month, 1)).OrderBy(g => g.Key).Select(g => new TrendPoint
        {
            Month = g.Key,
            Amount = g.Sum(t => Transaction.TransactionNetAmount(t.Id, t.Amount))
        });
    }
}
