using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Retirement.Commands;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Queries;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Retirement.Endpoints;

public class RetirementPlans : EndpointGroupBase
{
    public override string Path => "/retirement/plans";

    public override string? Tag => "Retirement";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetPlans, IEnumerable<RetirementPlan>>("/")
            .WithNames("Get All Retirement Plans");

        routeGroupBuilder.MapQuery<GetPlan, RetirementPlan>("/{id}")
            .WithNames("Get Retirement Plan")
            .RequireAuthorization(Policies.GetRetirementPlanPolicy());

        routeGroupBuilder.MapPostCreate<CreatePlan, RetirementPlan>("/", "Get Retirement Plan".ToMachine(), (plan) => new { id = plan.Id }, RequestBinding.Parameters)
            .WithNames("Create Retirement Plan")
            .WithValidation<CreatePlan>();

        routeGroupBuilder.MapPutCommand<UpdatePlan, RetirementPlan>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Retirement Plan")
            .RequireAuthorization(Policies.GetRetirementPlanPolicy())
            .WithValidation<UpdatePlan>();

        routeGroupBuilder.MapDeleteCommand<DeletePlan>("/{id}")
            .WithNames("Delete Retirement Plan")
            .RequireAuthorization(Policies.GetRetirementPlanPolicy());

        routeGroupBuilder.MapCommand<RunProjection, RetirementProjection>("/{planId}/run", binding: RequestBinding.Parameters)
            .WithNames("Run Retirement Projection")
            .RequireAuthorization(Policies.GetRetirementPlanPolicy("planId"));
    }
}
