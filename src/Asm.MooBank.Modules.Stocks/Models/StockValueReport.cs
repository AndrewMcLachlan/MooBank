namespace Asm.MooBank.Modules.Stocks.Models;

/// <summary>
/// Stock value report.
/// </summary>
/// <param name="InstrumentId">The instrument ID.</param>
/// <param name="Symbol">The stock symbol.</param>
/// <param name="Start">The start date of the report.</param>
/// <param name="End">The end date of the report.</param>
/// <param name="Granularity">How many days between each point in the report.></param>
public record StockValueReport(Guid InstrumentId, string Symbol, DateOnly Start, DateOnly End, int Granularity)
{
    public IList<StockValueReportPoint> Points { get; init; } = [];
    public IList<StockValueReportPoint> Investment { get; init; } = [];
}

public record StockValueReportPoint
{
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}
