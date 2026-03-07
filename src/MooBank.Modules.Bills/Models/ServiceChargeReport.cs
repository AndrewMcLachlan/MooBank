namespace Asm.MooBank.Modules.Bills.Models;

public record ServiceChargeReport
{
    public DateOnly Start { get; init; }

    public DateOnly End { get; init; }

    public IEnumerable<ServiceChargeDataPoint> DataPoints { get; init; } = [];
}

public record ServiceChargeDataPoint
{
    public DateOnly Date { get; init; }

    public string AccountName { get; init; } = String.Empty;

    public decimal AverageChargePerDay { get; init; }
}
