﻿using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Transactions.Endpoints;
internal class TransactionsEndpoints : EndpointGroupBase
{
    public override string Name => "Transactions";

    public override string Path => "accounts/{instrumentId}/transactions";

    public override string Tags => "Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Get, Transaction>("{pageSize}/{pageNumber}")
            .WithNames("Get Transactions");

        builder.MapPagedQuery<Get, Transaction>("{untagged:alpha?}/{pageSize?}/{pageNumber?}")
            .WithNames("Get Untagged Transactions");

        builder.MapQuery<Search, IEnumerable<Transaction>>("")
            .WithNames("Search Transactions");

        builder.MapCommand<Create, Transaction>("", CommandBinding.None)
            .WithNames("Create Transaction");

        builder.MapCommand<UpdateBalance, Transaction>("/balance-adjustment", CommandBinding.Parameters)
            .WithNames("Set Balance");

        builder.MapPatchCommand<UpdateTransaction, Transaction>("{id}", CommandBinding.None)
            .WithNames("Update Transaction");

        builder.MapPutCommand<AddTag, Transaction>("{id}/tag/{tagId}")
            .WithNames("Add Tag");

        builder.MapDelete<RemoveTag, Transaction>("{id}/tag/{tagId}")
            .WithNames("Remove Tag");
    }
}
