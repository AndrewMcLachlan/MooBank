using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Queries.Transactions;

internal static class IQueryableExtensions
{
    public static IQueryable<Transaction> IncludeAll(this IQueryable<Transaction> query) => query.Include(t => t.TransactionTags).Include(t => t.OffsetBy).Include(t => t.Offsets);

    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => (TransactionModel)t).ToListAsync(cancellationToken);

}