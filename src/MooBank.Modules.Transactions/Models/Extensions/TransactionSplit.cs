using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Models.Extensions;

public static class TransactionSplitExtensions
{
    public static TransactionSplit ToModel(this Domain.Entities.Transactions.TransactionSplit split) => new()
    {
        Id = split.Id,
        Amount = Math.Abs(split.Amount),
        Tags = split.Tags.Where(t => !t.Deleted).ToSimpleModel(),
        OffsetBy = split.OffsetBy.Select(t => t.ToOffsetByModel()),
    };

    public static IEnumerable<Domain.Entities.Transactions.SplitUpdate> ToSplitUpdates(this IEnumerable<TransactionSplit> splits) =>
        splits.Select(split => new Domain.Entities.Transactions.SplitUpdate
        {
            Id = split.Id,
            Amount = split.Amount,
            TagIds = split.Tags.Select(t => t.Id),
            OffsetBy = split.OffsetBy.Select(o => new Domain.Entities.Transactions.OffsetUpdate
            {
                TransactionId = o.Transaction.Id,
                Amount = o.Amount,
            }),
        });
}
