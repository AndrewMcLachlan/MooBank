using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Asm.MooBank.Queries.Transactions;

public static class IQueryableExtensions
{
    public static IQueryable<Transaction> IncludeAll(this IQueryable<Transaction> query) =>
        query.IncludeTags().IncludeOffsets().Include(t => t.AccountHolder);

    public static IIncludableQueryable<Transaction, Transaction> IncludeOffsets(this IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.OffsetBy).ThenInclude(to => to.OffsetByTransaction).Include(t => t.OffsetFor).ThenInclude(t => t.TransactionSplit).ThenInclude(t => t.Transaction);

    public static IIncludableQueryable<Transaction, ICollection<Domain.Entities.Tag.Tag>> IncludeTags(this IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.Tags);

    public static IIncludableQueryable<Transaction, ICollection<Domain.Entities.Tag.Tag>> IncludeTagsAndSubTags(this IQueryable<Transaction> query) =>
        query.IncludeTags().ThenInclude(t => t.Tags);

    // Not parsable by EF Core
    /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Domain.Entities.Tag.Tag> SelectTags(this Transaction transaction) =>
        transaction.TransactionSplits.SelectMany(t => t.Tags);*/

}
