#nullable enable
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Tag;
using Bogus;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;

namespace Asm.MooBank.Modules.Budgets.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainBudget CreateBudget(
        Guid? id = null,
        short year = 2024,
        Guid? familyId = null,
        IEnumerable<DomainBudgetLine>? lines = null)
    {
        var budgetId = id ?? Guid.NewGuid();
        var budget = new DomainBudget(budgetId)
        {
            Year = year,
            FamilyId = familyId ?? Guid.NewGuid(),
        };

        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.BudgetId = budgetId;
                budget.Lines.Add(line);
            }
        }

        return budget;
    }

    public static DomainBudgetLine CreateBudgetLine(
        Guid? id = null,
        Guid? budgetId = null,
        int tagId = 1,
        string tagName = "Test Tag",
        string? notes = null,
        decimal amount = 100m,
        bool income = false,
        short month = 4095)
    {
        var lineId = id ?? Guid.NewGuid();
        return new DomainBudgetLine(lineId)
        {
            BudgetId = budgetId ?? Guid.NewGuid(),
            TagId = tagId,
            Tag = CreateTag(tagId, tagName),
            Notes = notes,
            Amount = amount,
            Income = income,
            Month = month,
        };
    }

    public static Tag CreateTag(int id = 1, string name = "Test Tag")
    {
        return new Tag(id)
        {
            Name = name,
            FamilyId = Guid.NewGuid(),
        };
    }

    public static Models.BudgetLine CreateBudgetLineModel(
        Guid? id = null,
        int tagId = 1,
        string name = "Test Tag",
        string? notes = null,
        decimal amount = 100m,
        short month = 4095,
        Models.BudgetLineType type = Models.BudgetLineType.Expenses)
    {
        return new Models.BudgetLine
        {
            Id = id ?? Guid.NewGuid(),
            TagId = tagId,
            Name = name,
            Notes = notes,
            Amount = amount,
            Month = month,
            Type = type,
        };
    }

    public static IQueryable<DomainBudget> CreateBudgetQueryable(IEnumerable<DomainBudget> budgets)
    {
        return QueryableHelper.CreateAsyncQueryable(budgets);
    }

    public static IQueryable<DomainBudget> CreateBudgetQueryable(params DomainBudget[] budgets)
    {
        return CreateBudgetQueryable(budgets.AsEnumerable());
    }

    public static IQueryable<DomainBudgetLine> CreateBudgetLineQueryable(IEnumerable<DomainBudgetLine> lines)
    {
        return QueryableHelper.CreateAsyncQueryable(lines);
    }

    public static IQueryable<DomainBudgetLine> CreateBudgetLineQueryable(params DomainBudgetLine[] lines)
    {
        return CreateBudgetLineQueryable(lines.AsEnumerable());
    }
}
