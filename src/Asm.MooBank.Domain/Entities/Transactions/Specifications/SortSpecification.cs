using System.Linq.Expressions;
using System.Reflection;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

public class SortSpecification(ISortable sort) : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Sort(sort.SortField, sort.SortDirection);
}

file static class Extensions
{
    private static readonly PropertyInfo[] TransactionProperties;

    static Extensions()
    {
        TransactionProperties = typeof(Transaction).GetProperties();
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
}
