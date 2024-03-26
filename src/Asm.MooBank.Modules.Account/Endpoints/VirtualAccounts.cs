using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Commands.VirtualAccount;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Modules.Account.Queries.VirtualAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;
internal class VirtualAccounts : EndpointGroupBase
{
    public override string Name => "Virtual Accounts";

    public override string Path => "/accounts/{accountId}/virtual";

    public override string Tags => Name;

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetForAccount, IEnumerable<VirtualAccount>>("/")
            .WithNames("Get Virtual Accounts");

        builder.MapQuery<Get, VirtualAccount>("/{virtualAccountId}")
            .WithNames("Get Virtual Account");

        builder.MapPostCreate<Create, VirtualAccount>("/", "Get Virtual Account".ToMachine(), a => new { VirtualAccountId = a.Id }, CommandBinding.Parameters)
            .WithNames("Create Virtual Account");

        builder.MapPatchCommand<Update, VirtualAccount>("/{virtualAccountId}", CommandBinding.None)
            .WithNames("Update Virtual Account");

        builder.MapDelete<Delete>("/{virtualAccountId}")
            .WithNames("Delete Virtual Account");
    }
}
