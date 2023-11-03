namespace Asm.MooBank.Modules.Budget.Models;

public partial record BudgetLine
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int TagId { get; set; }

    public string? Notes { get; set; }

    public decimal Amount { get; set; }

    public short Month { get; set; }

    public BudgetLineType Type { get; set; }
}

public partial record BudgetLine
{
    public static implicit operator BudgetLine(Domain.Entities.Budget.BudgetLine budgetLine) =>
        new()
        {
            Id = budgetLine.Id,
            TagId = budgetLine.TagId,
            Name = budgetLine.Tag.Name,
            Notes = budgetLine.Notes,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Type = budgetLine.Income ? BudgetLineType.Income : BudgetLineType.Expenses,
        };

}

public static class BudgetLineExtensions
{
    public static Domain.Entities.Budget.BudgetLine ToDomain(this BudgetLine budgetLine, Guid budgetId) =>
        new(budgetLine.Id)
        {
            BudgetId = budgetId,
            TagId = budgetLine.TagId,
            Notes = budgetLine.Notes,
            Amount = budgetLine.Amount,
            Month = budgetLine.Month,
            Income = budgetLine.Type == BudgetLineType.Income,
        };

    public static IEnumerable<BudgetLine> ToModel(this IEnumerable<Domain.Entities.Budget.BudgetLine> budgetLines)
    {
        return budgetLines.Select(bl => (BudgetLine)bl);
    }
}