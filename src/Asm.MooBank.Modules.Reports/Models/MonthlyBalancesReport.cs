namespace Asm.MooBank.Modules.Reports.Models;

public record MonthlyBalancesReport : ReportBase
{
    public required IEnumerable<TrendPoint> Balances { get; init; } = [];
}
