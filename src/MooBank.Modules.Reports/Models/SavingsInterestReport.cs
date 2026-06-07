namespace Asm.MooBank.Modules.Reports.Models;

public record SavingsInterestReport : ReportBase
{
    public required int TagId { get; init; }

    public required string TagName { get; init; }

    public required IEnumerable<TrendPoint> Months { get; init; } = [];

    public required decimal Total { get; init; }

    public required decimal MonthlyAverage { get; init; }
}
