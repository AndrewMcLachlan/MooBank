using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Queries.Transactions;
using Microsoft.EntityFrameworkCore;
using PagedResult = Asm.MooBank.Models.PagedResult<Asm.MooBank.Models.Transaction>;
using TransactionModel = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Services.Queries.Transactions;

public class GetTransactionsHandler : IQueryHandler<GetTransactions, PagedResult>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurityRepository _security;


    public GetTransactionsHandler(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _security = securityRepository;
    }

    public async Task<PagedResult> Handle(GetTransactions request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var total = await _transactions.Include(t => t.TransactionTags).Where(request).CountAsync(cancellationToken);

        var results = await _transactions.Include(t => t.TransactionTags).Where(request).Sort(request.SortField, request.SortDirection).Page(request.PageSize, request.PageNumber).ToModelAsync(cancellationToken);

        return new PagedResult
        {
            Results = results,
            Total = total,
        };
    }
}

file static class IQueryableExtensions
{
    private static readonly PropertyInfo[] _transactionProperties;

    static IQueryableExtensions()
    {
        _transactionProperties = typeof(Transaction).GetProperties();
    }

    public static IQueryable<Transaction> Where(this IQueryable<Transaction> query, GetTransactions request)
    {
        var result = query.Where(t=> t.AccountId == request.AccountId);
        result = result.Where(t => request.Filter == null || (t.Description != null && t.Description.Contains(request.Filter)));
        result = result.Where(t => (request.Start == null || t.TransactionTime >= request.Start) && (request.End == null || t.TransactionTime <= request.End));
        result = result.Where(t => !request.UntaggedOnly || !t.TransactionTags.Any());
        result = result.Where(t => request.TagId == null || (t.TransactionTags.Any(t => t.TransactionTagId == request.TagId)));

        return result;
    }

    public static IOrderedQueryable<Transaction> Sort(this IQueryable<Transaction> query, string? field, SortDirection direction)
    {
        if (!String.IsNullOrWhiteSpace(field))
        {
            PropertyInfo? property = _transactionProperties.SingleOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));

            if (property == null) throw new ArgumentException($"Unknown field {field}", nameof(field));


            ParameterExpression param = Expression.Parameter(typeof(Transaction), String.Empty);
            MemberExpression propertyExp = Expression.Property(param, field);
            Expression sortBody = field == "Amount" ? Expression.Call(typeof(Math), "Abs", null, propertyExp) : propertyExp;
            LambdaExpression sort = Expression.Lambda(sortBody, param);
            MethodCallExpression call =
                Expression.Call(typeof(Queryable), "OrderBy" + (direction == SortDirection.Descending ? "Descending" : String.Empty), new[] { typeof(Transaction), propertyExp.Type },
                query.Expression,
                Expression.Quote(sort));
            return (IOrderedQueryable<Transaction>)query.Provider.CreateQuery<Transaction>(call);
        }

        Expression<Func<Transaction, object>> sortFunc = t => t.TransactionTime;
        return direction == SortDirection.Ascending ? query.OrderBy(sortFunc) : query.OrderByDescending(sortFunc);

    }

    public static async Task<IEnumerable<TransactionModel>> ToModelAsync(this IQueryable<Transaction> query, CancellationToken cancellationToken = default) =>
        await query.Select(t => (TransactionModel)t).ToListAsync(cancellationToken);

}
