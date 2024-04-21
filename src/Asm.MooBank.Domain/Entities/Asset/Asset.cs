namespace Asm.MooBank.Domain.Entities.Asset;

[AggregateRoot]
public class Asset(Guid id) : Account.Account(id)
{
    public Asset() : this(Guid.Empty)
    {
    }

    public decimal? PurchasePrice { get; set; }
}
