using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Stocks.Commands;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Modules.Stocks.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Stocks.Endpoints;
internal class StockHoldings : EndpointGroupBase
{
    public override string Name => "Stock Holdings";

    public override string Path => "/stock";

    public override string Tags => "Stock Holding";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, StockHolding>("/{id}")
            .WithNames("Get Stock Holding");

        builder.MapPostCreate<Create, StockHolding>("/", "Get Stock Holding".ToMachine(), a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Stock Holding");

        builder.MapPatchCommand<Update, StockHolding>("/{id}", CommandBinding.None)
            .WithNames("Update Stock Holding");

        builder.MapQuery<GetStockHoldingReport, StockHoldingReport>("stock-holding")
            .WithNames("Stock Holding Report");
    }
}
