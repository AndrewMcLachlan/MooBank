namespace Asm.MooBank.Modules.Budgets.Models;

public record BudgetReportValueTag(string Name, decimal BudgetedAmount, decimal? Actual) : BudgetReportValue(BudgetedAmount, Actual);
