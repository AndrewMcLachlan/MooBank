namespace Asm.MooBank.Modules.Bills.Models;

public record CostPerUnitReport
{
    public DateOnly Start { get; init; }

    public DateOnly End { get; init; }

    public IEnumerable<CostDataPoint> DataPoints { get; init; } = [];
}

public record CostDataPoint
{
    public DateOnly Date { get; init; }

    public string AccountName { get; init; } = string.Empty;

    public decimal AveragePricePerUnit { get; init; }

    public decimal TotalUsage { get; init; }
}
