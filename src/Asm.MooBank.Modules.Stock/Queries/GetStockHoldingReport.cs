﻿using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stock.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Stock.Queries;

public record GetStockHoldingReport(Guid AccountId) : IQuery<StockHoldingReport>;

internal class GetStockHoldingReportHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, ISecurity security, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetStockHoldingReport, StockHoldingReport>
{
    public async ValueTask<StockHoldingReport> Handle(GetStockHoldingReport query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var stockHolding = await stockHoldings.Include(s => s.Transactions).SingleAsync(s => s.Id == query.AccountId, cancellationToken);

        decimal profitLoss = 0;

        foreach (var transaction in stockHolding.Transactions)
        {
            var cost = transaction.Price * transaction.Quantity + transaction.Fees;

            profitLoss += transaction.Quantity * stockHolding.CurrentPrice;
        }

        return new()
        {
            AccountId = query.AccountId,
            CurrentValue = stockHolding.CurrentValue,
            ProfitLoss = profitLoss,
        };
    }
}
