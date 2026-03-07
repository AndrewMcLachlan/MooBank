namespace Asm.MooBank.Modules.Bills.Models;

public record UsageReport
{
    public DateOnly Start { get; init; }

    public DateOnly End { get; init; }

    public IEnumerable<UsageDataPoint> DataPoints { get; init; } = [];
}

public record UsageDataPoint
{
    public DateOnly Date { get; init; }

    public string AccountName { get; init; } = String.Empty;

    public decimal UsagePerDay { get; init; }
}
