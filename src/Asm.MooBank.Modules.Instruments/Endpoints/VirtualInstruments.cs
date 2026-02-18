using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;
using Asm.MooBank.Modules.Instruments.Models.Virtual;
using Asm.MooBank.Modules.Instruments.Queries.VirtualAccounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;
internal class VirtualInstruments : EndpointGroupBase
{
    public override string Name => "Virtual Instruments";

    public override string Path => "/instruments/{instrumentId}/virtual";

    public override string Tags => Name;

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetForAccount, IEnumerable<VirtualInstrument>>("/")
            .WithNames("Get Virtual Instruments");

        builder.MapQuery<Get, VirtualInstrument>("/{virtualInstrumentId}")
            .WithNames("Get Virtual Instrument");

        builder.MapPostCreate<Create, VirtualInstrument>("/", "Get Virtual Instrument".ToMachine(), a => new { VirtualInstrumentId = a.Id }, CommandBinding.Parameters)
            .WithNames("Create Virtual Instrument");

        builder.MapPatchCommand<Update, VirtualInstrument>("/{virtualInstrumentId}", CommandBinding.None)
            .WithNames("Update Virtual Instrument")
            .Accepts<Update>("application/json");

        builder.MapPatchCommand<UpdateBalance, VirtualInstrument>("/{virtualInstrumentId}/balance", CommandBinding.None)
            .WithNames("Update Virtual Instrument Balance")
            .Accepts<UpdateBalance>("application/json");

        builder.MapDelete<Delete>("/{virtualInstrumentId}")
            .WithNames("Delete Virtual Instrument");
    }
}
