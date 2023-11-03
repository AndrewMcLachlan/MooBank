using System.Runtime.CompilerServices;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore.Query;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Modules.Transactions.Queries;

internal static class IQueryableExtensions
{
    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => (TransactionModel)t).ToListAsync(cancellationToken);

}