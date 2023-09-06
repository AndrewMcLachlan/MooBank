﻿namespace Asm.MooBank.Models;

public partial record Budget
{
    public static implicit operator Budget(Domain.Entities.Budget.Budget? budget)
    {
        if (budget == null) return null!;

        var lines = budget.Lines.ToModel();
        List<Models.BudgetMonth> months = new();
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
}