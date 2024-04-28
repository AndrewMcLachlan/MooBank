using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Asset;

[AggregateRoot]
public class Asset(Guid id) : Instrument.Instrument(id)
{
    public Asset() : this(Guid.Empty)
    {
    }

    [Precision(12, 4)]
    public decimal? PurchasePrice { get; set; }
}
