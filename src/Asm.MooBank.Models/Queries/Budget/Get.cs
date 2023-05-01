namespace Asm.MooBank.Models.Queries.Budget;

public record Get(Guid AccountId, Guid Id) : IQuery<BudgetLine>;