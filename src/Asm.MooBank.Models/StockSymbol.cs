using System.Text.RegularExpressions;

namespace Asm.MooBank.Models;

/// <summary>
/// Note that this is wildly wrong.
/// https://en.wikipedia.org/wiki/Ticker_symbol
/// e.g. BRK.A:NYS, BRK/A
/// </summary>
/// <param name="symbol"></param>
/// <param name="exchange"></param>
public partial class StockSymbol(string symbol, string? exchange) : IEquatable<StockSymbol>
{
    [GeneratedRegex(@"\w*")]
    private static partial Regex AlphaNumeric();

    public string Symbol { get; init; } = symbol;

    public string? Exchange { get; init; } = exchange;

    public override string ToString() => Exchange != null ? $"{Symbol}.{Exchange}" : Symbol;

    public static StockSymbol Parse(string symbol)
    {
        var split = symbol.ToUpperInvariant().Split(".");
        if (split.Length > 2 || (split.Length == 2 && split[1].Length != 2)) throw new FormatException("Invalid symbol format");

        if (!AlphaNumeric().IsMatch(split[0])) throw new FormatException("Invalid symbol format");

        return new StockSymbol(split[0], split.Length > 1 ? split[1] : null);
    }

    public static bool TryParse(string symbol, out StockSymbol? result)
    {
        result = null;
        var split = symbol.Split(".");
        if (split.Length > 2 || split[1].Length != 2) return false;
        if (!AlphaNumeric().IsMatch(split[0])) return false;

        result = new StockSymbol(split[0], split.Length > 1 ? split[1] : null);
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

