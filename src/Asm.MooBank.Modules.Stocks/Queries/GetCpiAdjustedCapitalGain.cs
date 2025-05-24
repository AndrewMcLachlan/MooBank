using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Queries;

public record GetCpiAdjustedCapitalGain(Guid instrumentId) : IQuery<decimal>;

internal class GetCpiAdjustedCapitalGainHandler(IQueryable<StockHolding> stockHoldings, ICpiService cpiService) : IQueryHandler<GetCpiAdjustedCapitalGain, decimal>
{
    public async ValueTask<decimal> Handle(GetCpiAdjustedCapitalGain request, CancellationToken cancellationToken)
    {
        var stockHolding = await stockHoldings.Include(t => t.Transactions).Where(t => t.Id == request.instrumentId).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Stock holding with ID {request.instrumentId} not found.");

        decimal totalAdjustedGain = 0;

        foreach (var transaction in stockHolding.Transactions)
        {
            var amount = transaction.Quantity * transaction.Price;

            amount = transaction.TransactionType == MooBank.Models.TransactionType.Debit ? -amount : amount;

            totalAdjustedGain += await cpiService.CalculateAdjustedValue(amount, transaction.TransactionDate, cancellationToken);
        }

        return stockHolding.CurrentValue - totalAdjustedGain;
    }
}
