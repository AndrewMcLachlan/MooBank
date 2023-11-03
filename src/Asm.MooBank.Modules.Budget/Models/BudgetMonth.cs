namespace Asm.MooBank.Modules.Budget.Models;
public record BudgetMonth(int Month, decimal Income, decimal Expenses)
{
    public decimal Remainder => Income - Expenses;
}

