﻿using Asm.MooBank.Modules.Stocks.Models;

namespace Asm.MooBank.Modules.Stocks.Queries;

public record GetStockHoldingReport(Guid AccountId) : IQuery<StockHoldingReport>;

internal class GetStockHoldingReportHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, ISecurity security) : IQueryHandler<GetStockHoldingReport, StockHoldingReport>
{
    public async ValueTask<StockHoldingReport> Handle(GetStockHoldingReport query, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(query.AccountId);

        var stockHolding = await stockHoldings.Include(s => s.Transactions).SingleAsync(s => s.Id == query.AccountId, cancellationToken);

        return new()
        {
            AccountId = query.AccountId,
            CurrentValue = stockHolding.CurrentValue,
            ProfitLoss = stockHolding.GainLoss,
        };
    }
}
