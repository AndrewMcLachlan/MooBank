using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Queries.Transactions;

internal static class IQueryableExtensions
{
    public static IQueryable<Transaction> IncludeAll(this IQueryable<Transaction> query) =>
        query.Include(t => t.TransactionSplits).ThenInclude(ts => ts.Tags).Include(t => t.OffsetBy).ThenInclude(t => t.OffsetByTransaction).Include(t => t.Offsets).ThenInclude(t => t.Transaction);

    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => (TransactionModel)t).ToListAsync(cancellationToken);

}