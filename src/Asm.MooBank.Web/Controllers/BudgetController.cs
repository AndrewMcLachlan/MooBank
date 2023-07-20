using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Budget;
using Asm.MooBank.Queries.Budget;
using Asm.MooBank.Services.Commands.Budget;

namespace Asm.MooBank.Web.Controllers;

[Route("/api/accounts/{accountId}/[controller]")]
public class BudgetController : CommandQueryController
{
    public BudgetController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {
    }

    [HttpGet]
    public Task<IEnumerable<short>> GetYears(Guid accountId, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetYears(accountId), cancellationToken);


    [HttpGet("{year}")]
    public async Task<Budget> Get(Guid accountId, short year, CancellationToken cancellationToken = default)
    {
        var budget = await QueryDispatcher.Dispatch(new Get(accountId, year), cancellationToken) ??
                     await CommandDispatcher.Dispatch(new Create(accountId, year), cancellationToken);
        return budget;
    }

    /*
    [HttpGet("{id}")]
    public Task<BudgetLine> Get(Guid accountId, Guid id, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new Get(accountId, id), cancellationToken);
    */

    [HttpPost("{year}/lines")]
    public async Task<ActionResult<BudgetLine>> Create(Guid accountId, short year, [FromBody] BudgetLine budgetLine, CancellationToken cancellationToken = default)
    {
        var model = await CommandDispatcher.Dispatch(new CreateLine(accountId, year, budgetLine), cancellationToken);

        return CreatedAtAction("Get", model);
    }

    [HttpPatch("{year}/lines/{id}")]
    public async Task<ActionResult<BudgetLine>> Update(Guid accountId, short year, Guid id, [FromBody] BudgetLine budgetLine, CancellationToken cancellationToken = default)
    {
        if (id != budgetLine.Id) return BadRequest();

        return await CommandDispatcher.Dispatch(new UpdateLine(accountId, year, id, budgetLine), cancellationToken);
    }

    [HttpDelete("{year}/lines/{id}")]
    public async Task<ActionResult> Delete(Guid accountId, short year, Guid id, CancellationToken cancellationToken = default)
    {
        await CommandDispatcher.Dispatch(new DeleteLine(accountId, year, id), cancellationToken);

        return NoContent();
    }

    [HttpGet("tag/{tagId}")]
    public Task<decimal> GetValueForTag(Guid accountId, int tagId, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetValueForTag(accountId, tagId), cancellationToken);

}
