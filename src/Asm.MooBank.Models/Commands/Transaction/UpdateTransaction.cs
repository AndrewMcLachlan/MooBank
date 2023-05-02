namespace Asm.MooBank.Models.Commands.Transaction;

public record UpdateTransaction(Guid Id, string? Notes, Guid? OffsetByTransactionId) : ICommand<Models.Transaction>;
