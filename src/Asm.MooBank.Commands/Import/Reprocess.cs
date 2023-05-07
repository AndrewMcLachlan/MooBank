namespace Asm.MooBank.Commands.Import;

public record Reprocess(Guid AccountId) : ICommand;
