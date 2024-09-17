using Asm.MooBank.Modules.Stocks.Models;

namespace Asm.MooBank.Modules.Stocks.Queries;

public record GetStockValueReport(Guid Id, DateOnly Start, DateOnly End) : IQuery<StockValueReport>
{
}

internal class GetStockValueReportHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings,
                                          IQueryable<Domain.Entities.Transactions.StockTransaction> transactions,
                                          IQueryable<Domain.Entities.ReferenceData.StockPriceHistory> stockPriceHistories, ISecurity security)
    : IQueryHandler<GetStockValueReport, StockValueReport>
{
    public async ValueTask<StockValueReport> Handle(GetStockValueReport query, CancellationToken cancellationToken)
    {
       security.AssertInstrumentPermission(query.Id);

        var stockHolding = await stockHoldings.SingleAsync(s => s.Id == query.Id, cancellationToken);

        var selectedTransactions = await transactions.Where(transaction => transaction.AccountId == query.Id && transaction.TransactionDate < query.End.ToEndOfDay()).ToListAsync(cancellationToken);

        //var stocksOnDay1 = selectedTransactions.Where(s => s.TransactionDate <= query.Start.ToStartOfDay()).Sum(s => s.Quantity);

        var stockPriceHistory = await stockPriceHistories
            .Where(s => s.Date >= query.Start && s.Date <= query.End)
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);

        // Limit the number of points shown.
        var granularity = (int)Math.Round((query.End.ToEndOfDay() - query.Start.ToStartOfDay()).TotalDays / 365d);

        var date = query.Start;
        decimal stockPrice = 0;

        StockValueReport stockValueReport = new(query.Id, stockHolding.Symbol, query.Start, query.End, granularity);

        while (date <= query.End)
        {
            var stockTransactions = selectedTransactions.Where(s => s.TransactionDate <= date.ToEndOfDay());
            var investment = stockTransactions.Sum(s => (s.Quantity * s.Price) + s.Fees);
            var stockQuantity = stockTransactions.Sum(s => s.Quantity);
            stockPrice = stockPriceHistory.FirstOrDefault(s => s.Date == date)?.Price ?? stockPrice;
            var stockValue = stockQuantity * stockPrice;
            if (stockValue != 0)
            {
                stockValueReport.Points.Add(new()
                {
                    Date = date,
                    Value = stockValue,
                });
                stockValueReport.Investment.Add(new()
                {
                    Date = date,
                    Value = investment,
                });
            }
            date = date.AddDays(granularity);
        }

        return stockValueReport;
    }
}