using PagedResult = Asm.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public record GetUntagged : Get, IQuery<PagedResult>;

internal class GetUntaggedHandler(IQueryHandler<Get, PagedResult> getHandler) : IQueryHandler<GetUntagged, PagedResult>
{
    public ValueTask<PagedResult> Handle(GetUntagged query, CancellationToken cancellationToken) =>
        getHandler.Handle(query with { UntaggedOnly = true }, cancellationToken);
}
