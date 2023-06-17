namespace Asm.MooBank.Models;

public partial record BudgetLine
{
    public static implicit operator BudgetLine(Domain.Entities.Budget.BudgetLine budgetLine) =>
        new()
        {
            Id = budgetLine.Id,
            TagId = budgetLine.TagId,
            Name = budgetLine.Tag.Name,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Income,
        };

}

public static class BudgetLineExtensions
{
    public static Domain.Entities.Budget.BudgetLine ToDomain(this BudgetLine budgetLine, Guid budgetId) =>
        new(budgetLine.Id)
        {
            BudgetId = budgetId,
            TagId = budgetLine.TagId,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Income,
        };

    public static IEnumerable<BudgetLine> ToModel(this IEnumerable<Domain.Entities.Budget.BudgetLine> budgetLines)
    {
        return budgetLines.Select(bl => (BudgetLine)bl);
    }
}