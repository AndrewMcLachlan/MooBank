using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using PagedResult = Asm.PagedResult<Asm.MooBank.Modules.Transactions.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public sealed record Get : TransactionFilter, IQuery<PagedResult>
{
    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }
}

internal class GetHandler(IQueryable<Transaction> transactions) : IQueryHandler<Get, PagedResult>
{
    public async ValueTask<PagedResult> Handle(Get query, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(query.InstrumentId);

        var filterSpecification = new FilterSpecification(query);

        var total = await transactions.Specify(filterSpecification).CountAsync(cancellationToken);

        var results = await transactions.IncludeAll().Specify(filterSpecification).Specify(new SortSpecification(query)).Page(query.PageSize, query.PageNumber).ToModel().ToListAsync(cancellationToken);

        var result = new PagedResult
        {
            Results = results,
            Total = total,
        };

        return result;
    }
}
