using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Microsoft.IdentityModel.Tokens;
using PagedResult = Asm.PagedResult<Asm.MooBank.Modules.Transactions.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public sealed record Get : IQuery<PagedResult>
{
    private bool _untagged;

    public required Guid InstrumentId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public int[]? TagIds { get; set; }

    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public string? SortField { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    public string Untagged { init => _untagged = value == "untagged"; }

    public bool? UntaggedOnly { get => _untagged; }
}

internal class GetHandler(IQueryable<Transaction> transactions, ISecurity security) : IQueryHandler<Get, PagedResult>
{

    public async ValueTask<PagedResult> Handle(Get query, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(query.InstrumentId);

        var total = await transactions.Where(query).CountAsync(cancellationToken);

        var results = await transactions.IncludeAll().Where(query).Sort(query.SortField, query.SortDirection).Page(query.PageSize, query.PageNumber).ToModel().ToListAsync(cancellationToken);

        var result = new PagedResult
        {
            Results = results,
            Total = total,
        };

        return result;
    }
}

file static class IQueryableExtensions
{
    private static readonly PropertyInfo[] TransactionProperties;

    static IQueryableExtensions()
    {
        TransactionProperties = typeof(Models.Transaction).GetProperties();
    }

    public static IQueryable<Transaction> Where(this IQueryable<Transaction> queryable, Get query)
    {
        var result = queryable.Where(t => t.AccountId == query.InstrumentId);

        var filters = query.Filter?.Split(',') ?? [];

        if (!String.IsNullOrWhiteSpace(query.Filter))
        {
            var predicate = filters.Select(f => (Expression<Func<Transaction, bool>>)(t => t.Description != null && EF.Functions.Like(t.Description, $"%{f}%")));
            result = result.WhereAny(predicate);
        }

        result = result.Where(t => (query.Start == null || t.TransactionTime >= query.Start) && (query.End == null || t.TransactionTime <= query.End));
        result = result.Where(t => !(query.UntaggedOnly ?? false) || !t.Splits.SelectMany(ts => ts.Tags).Any());
        result = result.Where(t => query.TagIds.IsNullOrEmpty() || t.Splits.SelectMany(ts => ts.Tags).Any(t => query.TagIds!.Contains(t.Id)));

        return result;
    }

    public static IOrderedQueryable<Transaction> Sort(this IQueryable<Transaction> query, string? field, SortDirection direction)
    {
        if (!String.IsNullOrWhiteSpace(field))
        {
            PropertyInfo? property = TransactionProperties.SingleOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Unknown field {field}", nameof(field));

            // Hiding implementation details from the front-end
            if (field == "UserName") field = "User.FirstName";

            ParameterExpression param = Expression.Parameter(typeof(Transaction), String.Empty);

            Expression propertyExp = field.Split('.').Aggregate((Expression)param, Expression.Property);


            Expression sortBody = field == "Amount" ? Expression.Call(typeof(Math), "Abs", null, propertyExp) : propertyExp;

            LambdaExpression sort = Expression.Lambda(sortBody, param);
            MethodCallExpression call =
                Expression.Call(typeof(Queryable), "OrderBy" + (direction == SortDirection.Descending ? "Descending" : String.Empty), [typeof(Transaction), propertyExp.Type],
                query.Expression,
                Expression.Quote(sort));
            return (IOrderedQueryable<Transaction>)query.Provider.CreateQuery<Transaction>(call);
        }

        Expression<Func<Transaction, object>> sortFunc = t => t.TransactionTime;
        return direction == SortDirection.Ascending ? query.OrderBy(sortFunc) : query.OrderByDescending(sortFunc);

    }

    public static IQueryable<T> WhereAny<T>(this IQueryable<T> queryable, IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        var parameter = Expression.Parameter(typeof(T));
        return queryable.Where(Expression.Lambda<Func<T, bool>>(
            predicates.Aggregate<Expression<Func<T, bool>>, Expression>(
                null!,
                (current, predicate) =>
                {
                    var visitor = new ParameterSubstitutionVisitor(predicate.Parameters[0], parameter);
                    return current != null ? Expression.OrElse(current, visitor.Visit(predicate.Body)) : visitor.Visit(predicate.Body);
                }),
            parameter));
    }
}

file class ParameterSubstitutionVisitor(ParameterExpression source, ParameterExpression destination) : ExpressionVisitor
{
    private readonly ParameterExpression _destination = destination;
    private readonly ParameterExpression _source = source;

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return ReferenceEquals(node, _source) ? _destination : base.VisitParameter(node);
    }
}
