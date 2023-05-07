namespace Asm.MooBank.Models.Commands.Import;
public record Reprocess(Guid AccountId) : ICommand;
