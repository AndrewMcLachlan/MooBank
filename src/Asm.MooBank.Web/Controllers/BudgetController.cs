using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Budget;
using Asm.MooBank.Queries.Budget;

namespace Asm.MooBank.Web.Controllers;

[Route("/api/[controller]")]
public class BudgetController : CommandQueryController
{
    public BudgetController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {
    }

    [HttpGet]
    public Task<IEnumerable<short>> GetYears(CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetYears(), cancellationToken);


    [HttpGet("{year}")]
    public async Task<Budget> Get(short year, CancellationToken cancellationToken = default)
    {
        var budget = await QueryDispatcher.Dispatch(new Get(year), cancellationToken) ??
                     await CommandDispatcher.Dispatch(new Create(year), cancellationToken);
        return budget;
    }

    /*
    [HttpGet("{id}")]
    public Task<BudgetLine> Get(Guid accountId, Guid id, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new Get(accountId, id), cancellationToken);
    */

    [HttpGet("{year}/lines/{id}")]
    public Task<BudgetLine> Get(short year, Guid id, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetLine(year, id), cancellationToken);

    [HttpPost("{year}/lines")]
    public async Task<ActionResult<BudgetLine>> Create(short year, [FromBody] BudgetLine budgetLine, CancellationToken cancellationToken = default)
    {
        var model = await CommandDispatcher.Dispatch(new CreateLine(year, budgetLine), cancellationToken);

        return CreatedAtAction("Get", model);
    }

    [HttpPatch("{year}/lines/{id}")]
    public async Task<ActionResult<BudgetLine>> Update(short year, Guid id, [FromBody] BudgetLine budgetLine, CancellationToken cancellationToken = default)
    {
        if (id != budgetLine.Id) return BadRequest();

        return await CommandDispatcher.Dispatch(new UpdateLine(year, id, budgetLine), cancellationToken);
    }

    [HttpDelete("{year}/lines/{id}")]
    public async Task<ActionResult> Delete(short year, Guid id, CancellationToken cancellationToken = default)
    {
        await CommandDispatcher.Dispatch(new DeleteLine(year, id), cancellationToken);

        return NoContent();
    }

    [HttpGet("tag/{tagId}")]
    public Task<decimal> GetValueForTag(int tagId, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetValueForTag(tagId), cancellationToken);

}
