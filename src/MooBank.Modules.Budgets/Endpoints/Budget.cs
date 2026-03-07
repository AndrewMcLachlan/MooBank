using System.Net;
using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Budgets.Endpoints;

public class Budget : EndpointGroupBase
{
    public override string Name => "Budget";

    public override string Path => "/budget";

    public override string Tags => "Budget";


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
            .Produces<BudgetLine>();

        // HACK: How do we get year from the route into the response?
        routeGroupBuilder.MapPostCreate<CreateLine, BudgetLine>("/{year}/lines", "Get Budget Line".ToMachine(), (line) => new { year = 2023, id = line.Id }, CommandBinding.Parameters)
            .WithNames("Create Budget Line")
            .Produces<BudgetLine>();

        routeGroupBuilder.MapPatchCommand<UpdateLine, BudgetLine>("/{year}/lines/{id}")
            .WithNames("Update Budget Line");

        routeGroupBuilder.MapDelete<DeleteLine>("/{year}/lines/{id}")
            .WithNames("Delete Budget Line")
            .Produces((int)HttpStatusCode.NoContent);

        routeGroupBuilder.MapQuery<GetValueForTag, decimal>("tag/{tagId}")
            .WithNames("Get Budget Amount for Tag");
    }
}
