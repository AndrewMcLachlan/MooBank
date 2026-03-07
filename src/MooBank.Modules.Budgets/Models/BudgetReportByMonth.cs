namespace Asm.MooBank.Modules.Budgets.Models;

public record BudgetReportByMonth
{
    public IEnumerable<BudgetReportValueMonth> Items { get; init; } = Enumerable.Empty<BudgetReportValueMonth>();
}
