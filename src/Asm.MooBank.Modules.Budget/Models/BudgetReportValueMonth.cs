namespace Asm.MooBank.Modules.Budgets.Models;
public record BudgetReportValueMonth(decimal BudgetedAmount, decimal? Actual, int Month) : BudgetReportValue(BudgetedAmount, Actual);
