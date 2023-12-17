using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding(Guid id) : Account.Account(id)
{
    public string Symbol { get; set; } = null!;
    public int Quantity { get; set; }

    public decimal CurrentPrice { get; set; }

    public decimal CurrentValue { get; set; }

    public bool ShareWithFamily { get; set; }

    public new DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

    public ICollection<StockTransaction> Transactions { get; set; } = [];
}
