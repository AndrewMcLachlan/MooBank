using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Stocks.Commands.Transactions;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Modules.Stocks.Queries.StockTransactions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Stocks.Endpoints;

internal class StockTransactionsEndpoints : EndpointGroupBase
{
    public override string Name => "Stock Transactions";

    public override string Path => "/stocks/{instrumentId}/transactions";

    public override string Tags => "Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Get, StockTransaction>("{pageSize}/{pageNumber}")
            .WithNames("Get Stock Transactions");

        builder.MapCommand<Create, StockTransaction>("/", CommandBinding.None)
            .WithNames("Create Stock Transaction")
            .Accepts<Create>("application/json");
    }
}
