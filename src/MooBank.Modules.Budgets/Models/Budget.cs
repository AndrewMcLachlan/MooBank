namespace Asm.MooBank.Modules.Budgets.Models;

public sealed record Budget
{
    public required short Year { get; set; }

    public required IEnumerable<BudgetLine> IncomeLines { get; init; } = [];

    public required IEnumerable<BudgetLine> ExpensesLines { get; init; } = [];

    public required IEnumerable<BudgetMonth> Months { get; init; } = [];
}

public static class BudgetExtensions
{
    public static Domain.Entities.Budget.Budget ToDomain(this Budget budget) =>
        new(Guid.Empty)
        {
            Year = budget.Year,
        };

    public static Budget ToModel(this Domain.Entities.Budget.Budget? budget)
    {
        if (budget == null) return null!;

        var lines = budget.Lines.ToModel();
        List<BudgetMonth> months = [];
        for (int i = 0; i < 12; i++)
        {
            var mask = 1 << i;
            var monthIncome = lines.Where(l => l.Type == BudgetLineType.Income && (l.Month & 1 << i) != 0).Sum(l => l.Amount);
            var monthExpenses = lines.Where(l => l.Type == BudgetLineType.Expenses && (l.Month & 1 << i) != 0).Sum(l => l.Amount);

            months.Add(new(i, monthIncome, monthExpenses));
        }

        return new()
        {
            Year = budget.Year,
            IncomeLines = lines.Where(l => l.Type == BudgetLineType.Income).OrderBy(l => l.Name),
            ExpensesLines = lines.Where(l => l.Type == BudgetLineType.Expenses).OrderBy(l => l.Name),
            Months = months,
        };
    }

    public static IEnumerable<BudgetMonth> ToMonths(this Domain.Entities.Budget.Budget budget)
    {
        List<BudgetMonth> months = [];
        for (int i = 0; i < 12; i++)
        {
            var mask = 1 << i;
            var monthIncome = budget.Lines.Where(l => l.Income && (l.Month & 1 << i) != 0).Sum(l => l.Amount);
            var monthExpenses = budget.Lines.Where(l => !l.Income && (l.Month & 1 << i) != 0).Sum(l => l.Amount);

            // Add one to the month as the DateTime months are one-based
            months.Add(new(i + 1, monthIncome, monthExpenses));
        }

        return months;
    }

    public static IEnumerable<Domain.Entities.Budget.BudgetLine> WhereMonth(this IEnumerable<Domain.Entities.Budget.BudgetLine> lines, short month)
    {
        // Subtract one from the month as the DateTime months are one-based
        var mask = 1 << month - 1;

        return lines.Where(l => (l.Month & mask) != 0);
    }
}
