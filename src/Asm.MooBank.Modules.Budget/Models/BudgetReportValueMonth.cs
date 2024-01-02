namespace Asm.MooBank.Modules.Budget.Models;
public record BudgetReportValueMonth(decimal BudgetedAmount, decimal? Actual, int Month) : BudgetReportValue(BudgetedAmount, Actual);
