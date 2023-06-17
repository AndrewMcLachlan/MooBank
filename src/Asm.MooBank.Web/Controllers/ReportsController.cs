using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;
using Asm.MooBank.Queries.Reports;

namespace Asm.MooBank.Web.Controllers;

[Route("api/accounts/{accountId}/[controller]")]
[ApiController]
public class ReportsController : CommandQueryController
{
    public ReportsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {
    }

    [HttpGet("in-out/{start}/{end}")]
    public Task<InOutReport> GetInOut(Guid accountId, DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
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
    public Task<InOutTrendReport> GetInOutTrend(Guid accountId, DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
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
    public Task<ByTagReport> GetByTag(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int? parentTag, CancellationToken cancellationToken = default)
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
    public Task<BreakdownReport> GetBreakdown(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int? parentTag, CancellationToken cancellationToken = default)
    {
        GetBreakdownReport query = new(parentTag)
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = reportType,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }

    [HttpGet("{reportType}/tag-trend/{start}/{end}/{tag}")]
    public Task<TagTrendReport> GetTagTrend(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, int tag, [FromQuery]TagTrendReportSettings settings, CancellationToken cancellationToken = default)
    {
        GetTagTrendReport query = new(tag)
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = reportType,
            Settings = settings,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }

    [HttpGet("{reportType}/all-tag-average/{start}/{end}")]
    public Task<AllTagAverageReport> GetAllTagAverage(Guid accountId, DateOnly start, DateOnly end, ReportType reportType, CancellationToken cancellationToken = default)
    {
        GetAllTagAverageReport query = new()
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = reportType,
        };

        return QueryDispatcher.Dispatch(query, cancellationToken);
    }
}
