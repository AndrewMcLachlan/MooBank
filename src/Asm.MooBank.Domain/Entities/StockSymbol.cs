using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities;

[Owned]
public class StockSymbolEntity(string symbol, string? exchange)
{
    [MaxLength(5)]
    public string Symbol { get; init; } = symbol;

    [MaxLength(2)]
    public string? Exchange { get; init; } = exchange;

    public static implicit operator StockSymbol(StockSymbolEntity value)
        => new(value.Symbol, value.Exchange);

    public static implicit operator StockSymbolEntity(StockSymbol value)
        => new(value.Symbol, value.Exchange);

    public static implicit operator StockSymbolEntity(string symbol) => StockSymbol.Parse(symbol);

    public static implicit operator string(StockSymbolEntity symbol) => ((StockSymbol)symbol).ToString();
}
