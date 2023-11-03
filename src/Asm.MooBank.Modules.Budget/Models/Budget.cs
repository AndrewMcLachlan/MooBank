namespace Asm.MooBank.Modules.Budget.Models;

public partial record Budget
{
    public required short Year { get; set; }

    public required IEnumerable<BudgetLine> IncomeLines { get; init; } = Enumerable.Empty<BudgetLine>();

    public required IEnumerable<BudgetLine> ExpensesLines { get; init; } = Enumerable.Empty<BudgetLine>();

    public required IEnumerable<BudgetMonth> Months { get; init; } = Enumerable.Empty<BudgetMonth>();
}

public partial record Budget
{
    public static implicit operator Budget(Domain.Entities.Budget.Budget? budget)
    {
        if (budget == null) return null!;

        var lines = budget.Lines.ToModel();
        List<BudgetMonth> months = new();
        for (int i = 0; i < 12; i++)
        {
            var mask = 1 << i;
            var monthIncome = lines.Where(l => l.Type == BudgetLineType.Income && (l.Month & (1 << i)) != 0).Sum(l => l.Amount);
            var monthExpenses = lines.Where(l => l.Type == BudgetLineType.Expenses && (l.Month & (1 << i)) != 0).Sum(l => l.Amount);

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
}

public static class BudgetExtensions
{
    public static Domain.Entities.Budget.Budget ToDomain(this Budget budget) =>
        new(Guid.Empty)
        {
            Year = budget.Year,
        };

    public static IEnumerable<BudgetMonth> ToMonths(this Domain.Entities.Budget.Budget budget)
    {
        List<BudgetMonth> months = new();
        for (int i = 0; i < 12; i++)
        {
            var mask = 1 << i;
            var monthIncome = budget.Lines.Where(l => l.Income && (l.Month & (1 << i)) != 0).Sum(l => l.Amount);
            var monthExpenses = budget.Lines.Where(l => !l.Income && (l.Month & (1 << i)) != 0).Sum(l => l.Amount);

            months.Add(new(i, monthIncome, monthExpenses));
        }

        return months;
    }
}