using Asm.MooBank.Domain.Entities.Transactions;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Modules.Transactions.Queries;

internal static class IQueryableExtensions
{
    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => t.ToModel()).ToListAsync(cancellationToken);

}