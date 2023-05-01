namespace Asm.MooBank.Models.Queries.Budget;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<BudgetLine>>;
