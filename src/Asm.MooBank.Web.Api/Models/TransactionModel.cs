namespace Asm.MooBank.Web.Api.Models;

public record TransactionModel(string? Notes, IEnumerable<TransactionSplit> Splits);
