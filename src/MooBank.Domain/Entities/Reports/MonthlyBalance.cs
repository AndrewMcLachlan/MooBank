namespace Asm.MooBank.Domain.Entities.Reports;

public class MonthlyBalance
{
    public required DateOnly PeriodEnd { get; set; }

    public required decimal Balance { get; set; }
}
