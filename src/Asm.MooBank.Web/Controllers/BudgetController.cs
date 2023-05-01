using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Commands.Budget;
using Asm.MooBank.Models.Queries.Budget;

namespace Asm.MooBank.Web.Controllers
{
    [Route("/api/accounts/{accountId}/[controller]")]
    public class BudgetController : CommandQueryController
    {
        public BudgetController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
        {
        }

        [HttpGet]
        public Task<IEnumerable<BudgetLine>> GetAll(Guid accountId, CancellationToken cancellationToken = default) =>
            QueryDispatcher.Dispatch(new GetAll(accountId), cancellationToken);

        [HttpGet("{id}")]
        public Task<BudgetLine> Get(Guid accountId, Guid id, CancellationToken cancellationToken = default) =>
            QueryDispatcher.Dispatch(new Get(accountId, id), cancellationToken);

        [HttpPost]
        public async Task<ActionResult<BudgetLine>> Create(Guid accountId, [FromBody] BudgetLine budgetLine, CancellationToken cancellationToken = default)
        {
            var model = await CommandDispatcher.Dispatch(new Create(accountId, budgetLine), cancellationToken);

            return CreatedAtAction("Get", model);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<BudgetLine>> Update(Guid accountId, Guid id, [FromBody]BudgetLine budgetLine, CancellationToken cancellationToken = default)
        {
            if (id != budgetLine.Id) return BadRequest();

            return await CommandDispatcher.Dispatch(new Update(accountId, budgetLine), cancellationToken);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid accountId, Guid id, CancellationToken cancellationToken = default)
        {
            await CommandDispatcher.Dispatch<Delete>(new Delete(accountId, id), cancellationToken);

            return NoContent();
        }
    }
}
