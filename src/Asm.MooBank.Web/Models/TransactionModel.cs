using Asm.MooBank.Models.Commands.Transactions;

namespace Asm.MooBank.Web.Models;

public record TransactionModel(string? Notes, IEnumerable<TransactionSplit> Splits, IEnumerable<CreateOffset> OffsetBy);
