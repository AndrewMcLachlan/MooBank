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

}

public static class BudgetLineExtensions
{
    public static Domain.Entities.Budget.BudgetLine ToDomain(this BudgetLine budgetLine, Guid accountId) =>
        new(budgetLine.Id)
        {
            AccountId = accountId,
            TagId = budgetLine.TagId,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Income,
        };
}