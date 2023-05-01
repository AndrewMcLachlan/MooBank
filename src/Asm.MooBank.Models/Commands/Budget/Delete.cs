namespace Asm.MooBank.Models.Commands.Budget;

public record Delete(Guid AccountId, Guid Id) : ICommand;
