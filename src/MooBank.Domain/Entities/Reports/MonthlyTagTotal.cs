namespace Asm.MooBank.Domain.Entities.Reports;

public class MonthlyTagTotal
{
    public DateOnly Month { get; set; }

    public decimal GrossAmount { get; init; }

    public decimal NetAmount { get; init; }
}
