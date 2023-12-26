using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Modules.Stock.Models;
using Asm.MooBank.Modules.Stock.Queries.StockTransactions;
using PagedResult = Asm.PagedResult<Asm.MooBank.Modules.Stock.Models.StockTransaction>;

namespace Asm.MooBank.Modules.Stock.Queries.StockTransactions;

public sealed record Get : IQuery<PagedResult>
{
    public required Guid AccountId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public string? SortField { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;
}

internal class GetHandler(IQueryable<Domain.Entities.Transactions.StockTransaction> transactions, ISecurity security) : IQueryHandler<Get, PagedResult>
{
    public async ValueTask<PagedResult> Handle(Get query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var total = await transactions.Where(query).CountAsync(cancellationToken);

        var results = await transactions.Where(query).Sort(query.SortField, query.SortDirection).Page(query.PageSize, query.PageNumber).ToModel().ToListAsync(cancellationToken);

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
        TransactionProperties = typeof(Models.StockTransaction).GetProperties();
    }

    public static IQueryable<Domain.Entities.Transactions.StockTransaction> Where(this IQueryable<Domain.Entities.Transactions.StockTransaction> queryable, Get query)
    {
        var result = queryable.Where(t => t.AccountId == query.AccountId);

        result = result.Where(t => (query.Start == null || t.TransactionDate >= query.Start) && (query.End == null || t.TransactionDate <= query.End));

        return result;
    }

    public static IOrderedQueryable<Domain.Entities.Transactions.StockTransaction> Sort(this IQueryable<Domain.Entities.Transactions.StockTransaction> query, string? field, SortDirection direction)
    {
        if (!String.IsNullOrWhiteSpace(field))
        {
            PropertyInfo? property = TransactionProperties.SingleOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Unknown field {field}", nameof(field));

            // Hiding implementation details from the front-end
            if (field == "AccountHolder") field = "AccountHolder.FirstName";

            ParameterExpression param = Expression.Parameter(typeof(Domain.Entities.Transactions.StockTransaction), String.Empty);

            Expression propertyExp = field.Split('.').Aggregate((Expression)param, Expression.Property);


            Expression sortBody = field == "Amount" ? Expression.Call(typeof(Math), "Abs", null, propertyExp) : propertyExp;

            LambdaExpression sort = Expression.Lambda(sortBody, param);
            MethodCallExpression call =
                Expression.Call(typeof(Queryable), "OrderBy" + (direction == SortDirection.Descending ? "Descending" : String.Empty), [typeof(Domain.Entities.Transactions.StockTransaction), propertyExp.Type],
                query.Expression,
                Expression.Quote(sort));
            return (IOrderedQueryable<Domain.Entities.Transactions.StockTransaction>)query.Provider.CreateQuery<Domain.Entities.Transactions.StockTransaction>(call);
        }

        Expression<Func<Domain.Entities.Transactions.StockTransaction, object>> sortFunc = t => t.TransactionDate;
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