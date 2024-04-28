namespace Asm.MooBank.Modules.Budgets.Models;

public sealed record BudgetLine
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public int TagId { get; init; }

    public string? Notes { get; init; }

    public decimal Amount { get; init; }

    public short Month { get; init; }

    public BudgetLineType Type { get; init; }
}

public static class BudgetLineExtensions
{
    public static BudgetLine ToModel(this Domain.Entities.Budget.BudgetLine budgetLine) =>
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
        return budgetLines.Select(bl => bl.ToModel());
    }
}
