namespace Asm.MooBank.Models.Commands.Budget;

public record Update(Guid AccountId, BudgetLine BudgetLine) : ICommand<BudgetLine>;
