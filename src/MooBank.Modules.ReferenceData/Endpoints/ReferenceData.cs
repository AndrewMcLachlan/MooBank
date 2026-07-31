using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.ReferenceData.Models;
using Asm.MooBank.Modules.ReferenceData.Commands;
using Asm.MooBank.Modules.ReferenceData.Queries;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.ReferenceData.Endpoints;

internal class ReferenceData : EndpointGroupBase
{
    public override string Path => "/reference-data";

    public override string? Tag => "Reference Data";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetImporterTypes, IEnumerable<ImporterType>>("importer-types")
            .WithNames("Importer Types");

        builder.MapQuery<GetPensionRates, IEnumerable<PensionRates>>("pension-rates")
            .WithNames("Pension Rates");

        // Age Pension rates are national, so a change affects every family's projection — which puts
        // editing them behind the admin policy while reading them stays open to any signed-in user.
        builder.MapPutCommand<SavePensionRates, PensionRates>("pension-rates")
            .RequireAuthorization(Policies.Admin)
            .WithNames("Save Pension Rates")
            .WithValidation<SavePensionRates>();
    }
}
