﻿using Asm.MooBank.Modules.Stocks.Models;

namespace Asm.MooBank.Modules.Stocks.Queries;

public record GetStockHoldingReport(Guid InstrumentId) : IQuery<StockHoldingReport>;

internal class GetStockHoldingReportHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings) : IQueryHandler<GetStockHoldingReport, StockHoldingReport>
{
    public async ValueTask<StockHoldingReport> Handle(GetStockHoldingReport query, CancellationToken cancellationToken)
    {
        var stockHolding = await stockHoldings.Include(s => s.Transactions).SingleAsync(s => s.Id == query.InstrumentId, cancellationToken);

        return new()
        {
            AccountId = query.InstrumentId,
            CurrentValue = stockHolding.CurrentValue,
            ProfitLoss = stockHolding.GainLoss,
        };
    }
}
