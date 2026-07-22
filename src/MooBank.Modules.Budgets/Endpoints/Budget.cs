using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Budgets.Endpoints;

public class Budget : EndpointGroupBase
{
    public override string Path => "/budget";

    public override string? Tag => "Budget";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetYears, IEnumerable<short>>("/")
            .WithNames("Get All Budget Years")
            .Produces<IEnumerable<short>>();

        routeGroupBuilder.MapQuery<Get, Models.Budget?>("/{year}")
            .WithNames("Get Budget")
            .Produces<Models.Budget>();

        routeGroupBuilder.MapQuery<GetLine, BudgetLine>("/{year}/lines/{id}")
            .WithNames("Get Budget Line")
            .Produces<BudgetLine>()
            .RequireAuthorization(Policies.GetBudgetLinePolicy("id"));

        routeGroupBuilder.MapPostCreate<CreateLine, BudgetLine>("/{year}/lines", "Get Budget Line".ToMachine(), (command, line) => new { year = command.Year, id = line.Id }, RequestBinding.Parameters)
            .WithNames("Create Budget Line");

        routeGroupBuilder.MapCommand<GenerateBudget, Models.Budget>("/{year}/generate", binding: RequestBinding.Parameters)
            .WithNames("Generate Budget")
            .Produces<Models.Budget>();

        routeGroupBuilder.MapPatchCommand<UpdateLine, BudgetLine>("/{year}/lines/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Budget Line")
            .RequireAuthorization(Policies.GetBudgetLinePolicy("id"));

        routeGroupBuilder.MapDeleteCommand<DeleteLine>("/{year}/lines/{id}")
            .WithNames("Delete Budget Line")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(Policies.GetBudgetLinePolicy("id"));

        routeGroupBuilder.MapQuery<GetValueForTag, decimal>("tag/{tagId}")
            .WithNames("Get Budget Amount for Tag");
    }
}
