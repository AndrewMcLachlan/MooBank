namespace Asm.MooBank.Models;
public partial record Budget
{
    public required short Year { get; set; }

    public required IEnumerable<BudgetLine> IncomeLines { get; init; } = Enumerable.Empty<BudgetLine>();

    public required IEnumerable<BudgetLine> ExpensesLines { get; init; } = Enumerable.Empty<BudgetLine>();

    public required IEnumerable<BudgetMonth> Months { get; init; } = Enumerable.Empty<BudgetMonth>();
}
