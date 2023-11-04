using Asm.MooBank.Models;

namespace Asm.MooBank;

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
