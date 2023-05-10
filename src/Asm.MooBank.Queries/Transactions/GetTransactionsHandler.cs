using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Queries.Transactions;
using Microsoft.EntityFrameworkCore;
using PagedResult = Asm.MooBank.Models.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Queries.Transactions;

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
}