using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Models.Rules;
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

public class RulesEndpoints : EndpointGroupBase
{
    public override string Path => "instruments/{instrumentId}/rules";

    public override string? Tag => "Rules";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Rule>>("/")
             .WithNames("Get All Instrument Rules")
             .Produces<IEnumerable<Rule>>();

        routeGroupBuilder.MapQuery<Get, Rule>("/{ruleId}")
            .WithNames("Get Instrument Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapPostCreate<Create, Rule>("", "Get Instrument Rule".ToMachine(), (rule) => new { ruleId = rule.Id }, RequestBinding.Default)
            .WithNames("Create Instrument Rule")
            .Accepts<Create>("application/json")
            .Produces<Rule>();


        routeGroupBuilder.MapPatchCommand<Update, Rule>("/{ruleId}", binding: RequestBinding.Parameters)
            .WithNames("Update Instrument Rule")
            .Produces<Rule>();

        routeGroupBuilder.MapDeleteCommand<Delete>("/{ruleId}")
            .WithNames("Delete Instrument Rule");


        routeGroupBuilder.MapPutCommand<AddTag, Rule>("/{ruleId}/tag/{tagId}", binding: RequestBinding.Parameters)
            .WithNames("Add Tag to Instrument Rule")
            .Produces<Rule>()
            .RequireAuthorization(Policies.GetTagFamilyPolicy("tagId"));

        routeGroupBuilder.MapDeleteCommand<RemoveTag>("/{ruleId}/tag/{tagId}")
            .WithNames("Remove Tag from Instrument Rule")
            .RequireAuthorization(Policies.GetTagFamilyPolicy("tagId"));

        routeGroupBuilder.MapCommand<Run>("run", StatusCodes.Status202Accepted, RequestBinding.Parameters)
            .WithNames("Run rules");
    }
}
