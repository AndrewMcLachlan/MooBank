using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Accounts.Endpoints;

internal class Accounts : EndpointGroupBase
{
    public override string Path => "/accounts";

    public override string? Tag => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<LogicalAccount>>("/")
            .WithNames("Get Accounts");

        builder.MapQuery<Get, LogicalAccount>("/{instrumentId}")
            .WithNames("Get Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapPostCreate<Create, LogicalAccount>("/", "Get Account".ToMachine(), a => new { instrumentId = a.Id }, RequestBinding.Body)
            .WithNames("Create Account")
            .WithValidation<Create>();

        builder.MapPatchCommand<Update, LogicalAccount>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("id"))
            .WithValidation<Update>();

        builder.MapPutCommand<SetTagPurpose, LogicalAccount>("/{instrumentId}/tag-purposes/{purpose}/{tagId}", binding: RequestBinding.Parameters)
            .WithNames("Set Account Tag Purpose")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));

        builder.MapDeleteCommand<DeleteTagPurpose, LogicalAccount>("/{instrumentId}/tag-purposes/{purpose}")
            .WithNames("Delete Account Tag Purpose")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));
    }
}
