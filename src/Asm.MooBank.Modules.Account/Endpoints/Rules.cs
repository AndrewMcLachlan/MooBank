using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Commands.Rule;
using Asm.MooBank.Modules.Account.Queries.Rule;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;

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

        routeGroupBuilder.MapQuery<Get, Rule>("/{id}")
            .WithNames("Get Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapPostCreate<Create, Rule>("", "Get Account Rule", (Rule rule) => new { id = rule.Id })
            .WithNames("Create Account Rule")
            .Produces<Rule>();


        routeGroupBuilder.MapPatchCommand<Update, Rule>("/{id}")
            .WithNames("Update Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<Delete>("/{id}")
            .WithNames("Delete Account Rule");


        routeGroupBuilder.MapPutCommand<AddTag,Rule>("/{id}/tag/{tagId}")
            .WithNames("Add Tag to Account Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<RemoveTag>("/{id}/tag/{tagId}")
            .WithNames("Remove Tag from Account Rule");
    }
}
