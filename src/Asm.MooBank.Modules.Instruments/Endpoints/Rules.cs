using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Models.Rules;
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

public class RulesEndpoints : EndpointGroupBase
{
    public override string Name => "Rules";

    public override string Path => "instruments/{instrumentId}/rules";

    public override string Tags => "Rules";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Rule>>("/")
             .WithNames("Get All Instrument Rules")
             .Produces<IEnumerable<Rule>>();

        routeGroupBuilder.MapQuery<Get, Rule>("/{ruleId}")
            .WithNames("Get Instrument Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapPostCreate<Create, Rule>("", "Get Instrument Rule".ToMachine(), (rule) => new { ruleId = rule.Id }, CommandBinding.None)
            .WithNames("Create Instrument Rule")
            .Produces<Rule>();


        routeGroupBuilder.MapPatchCommand<Update, Rule>("/{ruleId}")
            .WithNames("Update Instrument Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<Delete>("/{ruleId}")
            .WithNames("Delete Instrument Rule");


        routeGroupBuilder.MapPutCommand<AddTag, Rule>("/{ruleId}/tag/{tagId}")
            .WithNames("Add Tag to Instrument Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDelete<RemoveTag>("/{ruleId}/tag/{tagId}")
            .WithNames("Remove Tag from Instrument Rule");

        routeGroupBuilder.MapCommand<Run>("run", StatusCodes.Status202Accepted, CommandBinding.Parameters)
            .WithNames("Run rules");
    }
}
