namespace Asm.MooBank.Models.Commands.Budget;

public record Create(Guid AccountId, BudgetLine BudgetLine) : ICommand<BudgetLine>;
