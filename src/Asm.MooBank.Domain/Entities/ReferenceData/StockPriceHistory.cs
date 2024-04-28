using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[AggregateRoot]
public class StockPriceHistory([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public StockPriceHistory() : this(default) { }

    public required StockSymbol Symbol { get; init; }

    public required DateOnly Date { get; init; }

    [Precision(12, 4)]
    public required decimal Price { get; init; }
}
