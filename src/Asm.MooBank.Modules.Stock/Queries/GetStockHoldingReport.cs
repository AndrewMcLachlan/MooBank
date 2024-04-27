using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Stocks.Queries;

public record GetStockHoldingReport(Guid AccountId) : IQuery<StockHoldingReport>;

internal class GetStockHoldingReportHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, ISecurity security, User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetStockHoldingReport, StockHoldingReport>
{
    public async ValueTask<StockHoldingReport> Handle(GetStockHoldingReport query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var stockHolding = await stockHoldings.Include(s => s.Transactions).SingleAsync(s => s.Id == query.AccountId, cancellationToken);

        return new()
        {
            AccountId = query.AccountId,
            CurrentValue = stockHolding.CurrentValue,
            ProfitLoss = stockHolding.GainLoss,
        };
    }
}
