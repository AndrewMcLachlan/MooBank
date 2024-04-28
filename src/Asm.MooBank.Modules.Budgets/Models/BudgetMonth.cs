namespace Asm.MooBank.Modules.Budgets.Models;
public record BudgetMonth(int Month, decimal Income, decimal Expenses)
{
    public decimal Remainder => Income - Expenses;
}

