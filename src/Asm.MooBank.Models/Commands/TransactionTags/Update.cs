namespace Asm.MooBank.Models.Commands.TransactionTags;

public record Update(int TagId, string Name, bool ExcludeFromReporting, bool ApplySmoothing) : ICommand<TransactionTag>;
