using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Modules.Stocks.Queries.StockTransactions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Stocks.Endpoints;
internal class StockTransactionsEndpoints : EndpointGroupBase
{
    public override string Name => "Stock Transactions";

    public override string Path => "/stock/{accountId}/transactions";

    public override string Tags => "Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Get, StockTransaction>("{pageSize}/{pageNumber}")
            .WithNames("Get Stock Transactions");
    }
}
