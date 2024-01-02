namespace Asm.MooBank.Modules.Budget.Models;
public record BudgetReportValueTag(string Name, decimal BudgetedAmount, decimal? Actual) : BudgetReportValue(BudgetedAmount, Actual);
