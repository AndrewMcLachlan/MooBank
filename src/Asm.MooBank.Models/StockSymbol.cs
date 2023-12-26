namespace Asm.MooBank.Models;
public class StockSymbol(string symbol, string exchange) : IEquatable<StockSymbol>
{
    public string Symbol { get; init; } = symbol;

    public string Exchange { get; init; } = exchange;

    public override string ToString() => $"{Symbol}.{Exchange}";

    public static StockSymbol Parse(string symbol)
    {
        var split = symbol.Split(".");
        if (split.Length != 2 || split[0].Length != 3 || split[1].Length != 2) throw new FormatException("Invalid symbol format");

        return new StockSymbol(split[0], split[1]);
    }

    public static bool TryParse(string symbol, out StockSymbol result)
    {
        result = null!;
        var split = symbol.Split(".");
        if (split.Length != 2 || split[0].Length != 3 || split[1].Length != 2) return false;

        result = new StockSymbol(split[0], split[1]);
        return true;
    }

    public override bool Equals(object? obj) =>
        Equals(obj as StockSymbol);

    public override int GetHashCode() =>
        HashCode.Combine(Symbol, Exchange);

    public bool Equals(StockSymbol? other)
    {
        if (other is null) return false;
        return Symbol == other.Symbol && Exchange == other.Exchange;
    }

    public static implicit operator StockSymbol(string symbol) => Parse(symbol);

    public static implicit operator string(StockSymbol symbol) => symbol.ToString();

    public static bool operator ==(StockSymbol a, StockSymbol b) => a.Equals(b);

    public static bool operator !=(StockSymbol a, StockSymbol b) => !a.Equals(b);
}

