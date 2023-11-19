using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;

namespace Asm.MooBank.Modules.Transactions;

public static class TransactionSplitExtensions
{
    public static TransactionSplit ToModel(this Domain.Entities.Transactions.TransactionSplit split) => new()
    {
        Id = split.Id,
        Amount = Math.Abs(split.Amount),
        Tags = split.Tags.Where(t => !t.Deleted).ToSimpleModel(),
        OffsetBy = split.OffsetBy.Select(t => t.ToOffsetByModel()),
    };
}
