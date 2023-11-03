﻿using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Transactions.Endpoints;
internal class TransactionsEndpoints : EndpointGroupBase
{
    public override string Name => "Transactions";

    public override string Path => "/account/{accountId}/transactions";

    public override string Tags => "Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Get, Transaction>("{pageSize?}/{pageNumber?}")
            .WithNames("Get Transactions");

        builder.MapPagedQuery<Get, Transaction>("untagged/{pageSize?}/{pageNumber?}")
            .WithNames("Get Untagged Transactions");

        builder.MapQuery<Search, IEnumerable<Transaction>>("search")
            .WithNames("Search Transactions");

        builder.MapPatchCommand<UpdateTransaction, Transaction>("{id}")
            .WithNames("Get Transaction");

        builder.MapPutCommand<AddTag, Transaction>("{id}/tag/{tagId}")
            .WithNames("Add Tag");

        builder.MapDelete<RemoveTag, Transaction>("{id}/tag/{tagId}")
            .WithNames("Remove Tag");
    }
}
