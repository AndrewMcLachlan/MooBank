using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Asm.MooBank.Domain.Entities.Transactions;

public static class IQueryableExtensions
{
    public static IQueryable<Transaction> IncludeAll(this IQueryable<Transaction> query) =>
        query.IncludeTags().Include(t => t.User).Specify(new IncludeSplitsAndOffsetsSpecification());

    public static IIncludableQueryable<Transaction, ICollection<Tag.Tag>> IncludeTags(this IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.Tags);

    public static IIncludableQueryable<Transaction, ICollection<Tag.Tag>> IncludeTagsAndSubTags(this IQueryable<Transaction> query) =>
        query.IncludeTags().ThenInclude(t => t.Tags);

    // Not parsable by EF Core
    /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Domain.Entities.Tag.Tag> SelectTags(this Transaction transaction) =>
        transaction.TransactionSplits.SelectMany(t => t.Tags);*/

}
