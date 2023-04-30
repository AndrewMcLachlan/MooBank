namespace Asm.MooBank.Models.Commands.Transaction;

public record SetTransactionNotes(Guid Id, string? Notes) : ICommand<Models.Transaction>;
