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

    public static IEnumerable<Domain.Entities.Transactions.TransactionSplit> ToEntities(this IEnumerable<TransactionSplit> splits) =>
        splits.Select(split => new Domain.Entities.Transactions.TransactionSplit(split.Id)
        {
            Amount = split.Amount,
            // Name stays null: ExistingTagByIdInterceptor reads a nameless Tag as a reference to an
            // existing row and leaves that row alone. A name here would be written.
            Tags = [.. split.Tags.Select(tag => new Domain.Entities.Tag.Tag(tag.Id) { Name = null! })],
            OffsetBy = [.. split.OffsetBy.Select(offset => new Domain.Entities.Transactions.TransactionOffset
            {
                OffsetTransactionId = offset.Transaction.Id,
                Amount = offset.Amount,
            })],
        });
}
