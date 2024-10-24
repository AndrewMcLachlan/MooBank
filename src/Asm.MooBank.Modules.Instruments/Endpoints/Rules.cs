﻿using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Rule;
using Asm.MooBank.Modules.Instruments.Queries.Rule;
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

public class RulesEndpoints : EndpointGroupBase
{
    public override string Name => "Account Rules";

    public override string Path => "accounts/{accountId}/rules";

    public override string Tags => "Account Rules";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Rule>>("/")
             .WithNames("Get All Account Rules")
             .Produces<IEnumerable<Rule>>();

        routeGroupBuilder.MapQuery<Get, Rule>("/{ruleId}")
            .WithNames("Get Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapPostCreate<Create, Rule>("", "Get Account Rule".ToMachine(), (rule) => new { ruleId = rule.Id }, CommandBinding.None)
            .WithNames("Create Account Rule")
            .Produces<Rule>();


        routeGroupBuilder.MapPatchCommand<Update, Rule>("/{ruleId}")
            .WithNames("Update Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<Delete>("/{ruleId}")
            .WithNames("Delete Account Rule");


        routeGroupBuilder.MapPutCommand<AddTag, Rule>("/{ruleId}/tag/{tagId}")
            .WithNames("Add Tag to Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<RemoveTag>("/{ruleId}/tag/{tagId}")
            .WithNames("Remove Tag from Account Rule");

        routeGroupBuilder.MapCommand<Run>("run", StatusCodes.Status202Accepted, CommandBinding.Parameters)
            .WithNames("Run rules");
    }
}
