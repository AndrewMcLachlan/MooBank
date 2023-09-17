using System.Runtime.CompilerServices;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore.Query;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Queries.Transactions;

internal static class IQueryableExtensions
{
    public static IQueryable<Transaction> IncludeAll(this IQueryable<Transaction> query) =>
        query.IncludeTags().Include(t => t.OffsetBy).ThenInclude(t => t.OffsetByTransaction).Include(t => t.Offsets).ThenInclude(t => t.Transaction);

    public static IIncludableQueryable<Transaction, ICollection<Domain.Entities.Tag.Tag>> IncludeTags(this IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.Tags);

    public static IIncludableQueryable<Transaction, ICollection<Domain.Entities.Tag.Tag>> IncludeTagsAndSubTags(this IQueryable<Transaction> query) =>
        query.IncludeTags().ThenInclude(t => t.Tags);

    // Not parsable by EF Core
    /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Domain.Entities.Tag.Tag> SelectTags(this Transaction transaction) =>
        transaction.TransactionSplits.SelectMany(t => t.Tags);*/


    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => (TransactionModel)t).ToListAsync(cancellationToken);

}