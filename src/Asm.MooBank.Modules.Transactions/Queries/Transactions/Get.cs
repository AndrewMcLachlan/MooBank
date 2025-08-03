using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using PagedResult = Asm.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public sealed record Get : TransactionFilter, IQuery<PagedResult>
{
    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }
}

internal class GetHandler(IQueryable<Domain.Entities.Transactions.Transaction> transactions) : IQueryHandler<Get, PagedResult>
{
    public async ValueTask<PagedResult> Handle(Get query, CancellationToken cancellationToken)
    {
        var filterSpecification = new FilterSpecification(query);

        var total = await transactions.Specify(filterSpecification).CountAsync(cancellationToken);

        query = MapSortFieldNames(query);

        var results = await transactions.IncludeAll().Specify(filterSpecification).Specify(new SortSpecification(query)).Page(query.PageSize, query.PageNumber).ToModel().ToListAsync(cancellationToken);

        var result = new PagedResult
        {
            Results = results,
            Total = total,
        };

        return result;
    }

    private static Get MapSortFieldNames(Get filter)
    {
        return filter.SortField switch
        {
            "AccountHolderName" => filter with { SortField = "User.FirstName" },
            _ => filter,
        };
    }

}
