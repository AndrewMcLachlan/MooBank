using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Stocks.Commands;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Modules.Stocks.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Stocks.Endpoints;
internal class StockHoldings : EndpointGroupBase
{
    public override string Name => "Stock Holdings";

    public override string Path => "/stocks";

    public override string Tags => "Stock Holding";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, StockHolding>("/{instrumentId}")
            .WithNames("Get Stock Holding")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapPostCreate<Create, StockHolding>("/", "Get Stock Holding".ToMachine(), a => new { InstrumentId = a.Id }, CommandBinding.Body)
            .WithNames("Create Stock Holding");

        builder.MapPatchCommand<Update, StockHolding>("/{instrumentId}", CommandBinding.None)
            .WithNames("Update Stock Holding")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapQuery<GetStockHoldingReport, StockHoldingReport>("{instrumentId}/reports/holding")
            .WithNames("Stock Holding Report")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapQuery<GetStockValueReport, StockValueReport>("{instrumentId}/reports/value")
            .WithNames("Stock Value Report")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());
    }
}
