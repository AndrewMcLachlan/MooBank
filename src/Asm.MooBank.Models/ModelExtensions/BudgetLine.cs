namespace Asm.MooBank.Models;

public partial record BudgetLine
{
    public static implicit operator BudgetLine(Domain.Entities.Budget.BudgetLine budgetLine) =>
        new()
        {
            TagId = budgetLine.TagId,
            Name = budgetLine.Tag.Name,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Income,
        };

    public static implicit operator Domain.Entities.Budget.BudgetLine(BudgetLine budgetLine) =>
        new(budgetLine.Id)
        {
            TagId = budgetLine.TagId,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Income,
        };
}
