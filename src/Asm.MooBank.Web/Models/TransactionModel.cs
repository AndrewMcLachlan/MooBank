namespace Asm.MooBank.Web.Models;

public record TransactionModel(string? Notes, IEnumerable<TransactionSplit> Splits);
