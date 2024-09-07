using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class StockPriceHistory([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public StockPriceHistory() : this(default) { }

    public required StockSymbol Symbol { get; init; }

    public required DateOnly Date { get; init; }

    [Precision(12, 4)]
    public required decimal Price { get; init; }
}
