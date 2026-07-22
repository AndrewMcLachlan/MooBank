using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries.InstitutionAccounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Accounts.Endpoints;

internal class InstitutionAccounts : EndpointGroupBase
{
    public override string Path => "/accounts/{instrumentId}/institution-accounts";

    public override string? Tag => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, InstitutionAccount>("/{id}")
            .WithNames("Get Institution Account");


        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Institution Account".ToMachine(), a => new { id = a.Id }, RequestBinding.Parameters)
            .WithNames("Create Institution Account");

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Institution Account");

        builder.MapCommand<Close, InstitutionAccount>("/{id}/close", binding: RequestBinding.Parameters)
            .WithNames("Close Institution Account");
    }
}
