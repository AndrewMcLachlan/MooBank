using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Transactions.Endpoints;

internal class TransactionsEndpoints : EndpointGroupBase
{
    public override string Path => "accounts/{instrumentId}/transactions";

    public override string? Tag => "Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Get, Transaction>("{pageSize}/{pageNumber}")
            .WithNames("Get Transactions");

        builder.MapPagedQuery<GetUntagged, Transaction>("untagged/{pageSize}/{pageNumber}")
            .WithNames("Get Untagged Transactions");

        builder.MapQuery<Search, IEnumerable<Transaction>>("")
            .WithNames("Search Transactions");

        builder.MapCommand<Create, Transaction>("", binding: RequestBinding.Parameters)
            .WithNames("Create Transaction")
            .Accepts<Models.CreateTransaction>("application/json");

        builder.MapCommand<UpdateBalance, Transaction>("/balance-adjustment", binding: RequestBinding.Parameters)
            .WithNames("Set Balance");

        builder.MapPatchCommand<UpdateTransaction, Transaction>("{id}", binding: RequestBinding.Default)
            .WithNames("Update Transaction")
            .Accepts<UpdateTransaction>("application/json");

        builder.MapPutCommand<AddTag, Transaction>("{id}/tag/{tagId}", binding: RequestBinding.Parameters)
            .WithNames("Add Tag");

        builder.MapDeleteCommand<RemoveTag, Transaction>("{id}/tag/{tagId}")
            .WithNames("Remove Tag");
    }
}
