namespace Asm.MooBank.Models.Reports;

public record TrendPoint
{
    public required DateOnly Month { get; set; }

    public required decimal Amount { get; init; }

}