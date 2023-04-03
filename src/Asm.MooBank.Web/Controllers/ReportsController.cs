using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Queries.Reports;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/accounts/{accountId}/[controller]")]
    [ApiController]
    public class ReportsController : CommandQueryController
    {
        public ReportsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
        {
        }

        [HttpGet("in-out/{start}/{end}")]
        public Task<InOutReport> GetInOut(Guid accountId, DateOnly start, DateOnly end, CancellationToken cancellationToken)
        {
            GetInOutReport query = new()
            {
                AccountId = accountId,
                Start = start,
                End = end
            };

            return QueryDispatcher.Dispatch(query, cancellationToken);
        }
    }
}
