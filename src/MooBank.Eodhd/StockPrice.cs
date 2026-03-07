using System.Text.Json.Serialization;

namespace Asm.MooBank.Eodhd;

internal record StockPrice
{
    public DateOnly Date { get; set; }

    public decimal Open { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public decimal Close { get; set; }

    [JsonPropertyName("adjusted_close")]
    public decimal AdjustedClose { get; set; }

    public int Volume { get; set; }
}
