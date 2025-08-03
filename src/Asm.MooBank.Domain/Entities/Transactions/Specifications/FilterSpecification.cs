using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

public class FilterSpecification(TransactionFilter filter) : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Where(filter);
}

file static class Extensions
{
    public static IQueryable<Transaction> Where(this IQueryable<Transaction> queryable, TransactionFilter filter)
    {
        var result = queryable.Where(t => t.AccountId == filter.InstrumentId);

        var filters = filter.Filter?.Split(',') ?? [];

        if (!String.IsNullOrWhiteSpace(filter.Filter))
        {
            var predicate = filters.Select(f => (Expression<Func<Transaction, bool>>)(t => t.Description != null && EF.Functions.Like(t.Description, $"%{f}%")));
            result = result.WhereAny(predicate);
        }

        if (filter.TransactionType is not null && filter.TransactionType != TransactionFilterType.None)
        {
            result = result.Where(t => (filter.TransactionType == TransactionFilterType.Debit && t.Amount < 0) || (filter.TransactionType == TransactionFilterType.Credit && t.Amount > 0));
        }

        result = result.Where(t => (filter.Start == null || t.TransactionTime >= filter.Start) && (filter.End == null || t.TransactionTime <= filter.End));
        result = result.Where(t => !(filter.UntaggedOnly ?? false) || !t.Splits.SelectMany(ts => ts.Tags).Any());
        result = result.Where(t => !(filter.ExcludeNetZero ?? false) || Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount) != 0);
        result = result.Where(t => filter.TagIds.IsNullOrEmpty() || t.Splits.SelectMany(ts => ts.Tags).Any(t => filter.TagIds!.Contains(t.Id)));

        return result;
    }
}
