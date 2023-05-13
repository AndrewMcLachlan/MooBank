using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Queries.Transactions;
using PagedResult = Asm.MooBank.Models.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Queries.Transactions;

public record GetTransactions : IQuery<PagedResult>
{
    public required Guid AccountId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public int? TagId { get; set; }

    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public string? SortField { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    public required bool UntaggedOnly { get; init; } = false;
}

public class GetTransactionsHandler : IQueryHandler<GetTransactions, PagedResult>
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<Account> _accounts;
    private readonly ISecurity _security;
    private readonly IImporterFactory _importerFactory;


    public GetTransactionsHandler(IQueryDispatcher queryDispatcher, IQueryable<Transaction> transactions, IQueryable<Account> accounts, ISecurity securityRepository, IImporterFactory importerFactory)
    {
        _queryDispatcher = queryDispatcher;
        _transactions = transactions;
        _accounts = accounts;
        _security = securityRepository;
        _importerFactory = importerFactory;
    }

    public async Task<PagedResult> Handle(GetTransactions request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);


        var total = await _transactions.Where(request).CountAsync(cancellationToken);

        var results = await _transactions.IncludeAll().Where(request).Sort(request.SortField, request.SortDirection).Page(request.PageSize, request.PageNumber).ToModelAsync(cancellationToken);

        var result = new PagedResult
        {
            Results = results,
            Total = total,
        };

        var importer = await _importerFactory.Create(request.AccountId, cancellationToken);

        if (importer != null)
        {
            var extraRequest = importer.CreateExtraDetailsRequest(request.AccountId, result);

            if (extraRequest != null)
            {
                result = await _queryDispatcher.Dispatch(extraRequest, cancellationToken);
            }
        }

        return result;
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
        var result = query.Where(t => t.AccountId == request.AccountId);

        var filters = request.Filter?.Split(',') ?? Array.Empty<string>();

        if (!String.IsNullOrWhiteSpace(request.Filter))
        {
            var predicate = filters.Select(f => (Expression<Func<Transaction, bool>>)(t => t.Description != null && EF.Functions.Like(t.Description, $"%{f}%")));
            result = result.WhereAny(predicate);
        }

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

file class ParameterSubstitutionVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _destination;
    private readonly ParameterExpression _source;

    public ParameterSubstitutionVisitor(ParameterExpression source, ParameterExpression destination)
    {
        _source = source;
        _destination = destination;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return ReferenceEquals(node, _source) ? _destination : base.VisitParameter(node);
    }
}