namespace Asm.MooBank.Modules.Budget.Models;
public record BudgetReportByMonth
{
    public IEnumerable<BudgetReportValueMonth> Items { get; init; } = Enumerable.Empty<BudgetReportValueMonth>();
}