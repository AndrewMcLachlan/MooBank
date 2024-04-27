namespace Asm.MooBank.Modules.Stocks.Models;
public record StockHoldingReport
{
    public Guid AccountId { get; set; }

    public decimal CurrentValue { get; set; }

    public decimal ProfitLoss { get; set; }
}
