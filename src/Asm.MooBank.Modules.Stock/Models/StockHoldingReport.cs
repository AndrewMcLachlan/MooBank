namespace Asm.MooBank.Modules.Stock.Models;
public record StockHoldingReport
{
    public Guid AccountId { get; set; }

    public decimal CurrentValue { get; set; }

    public decimal ProfitLoss { get; set; }
}
