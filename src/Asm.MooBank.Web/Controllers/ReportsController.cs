using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Queries.Reports;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Web.Controllers;

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

    [HttpGet("in-out-trend/{start}/{end}")]
    public Task<InOutTrendReport> GetInOutTrend(Guid accountId, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        GetInOutTrendReport query = new()
        {
            AccountId = accountId,
            Start = start,
            End = end
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }


    [HttpGet("{reportType}/tags/{start}/{end}/{parentTag?}")]
    public Task<ByTagReport> GetByTag(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int? parentTag, CancellationToken cancellationToken)
    {
        GetByTagReport query = new()
        {
            AccountId = accountId,
            Start = start,
            End = end,
            TagId = parentTag,
            ReportType = reportType,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }

    [HttpGet("{reportType}/breakdown/{start}/{end}/{parentTag?}")]
    public Task<BreakdownReport> GetBreakdown(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int? parentTag, CancellationToken cancellationToken)
    {
        GetBreakdownReport query = new()
        {
            AccountId = accountId,
            Start = start,
            End = end,
            TagId = parentTag,
            ReportType = reportType,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }

    [HttpGet("{reportType}/tag-trend/{start}/{end}/{tag}")]
    public Task<TagTrendReport> GetTagTrend(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int tag, CancellationToken cancellationToken)
    {
        GetTagTrendReport query = new()
        {
            AccountId = accountId,
            Start = start,
            End = end,
            TagId = tag,
            ReportType = reportType,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }
}
