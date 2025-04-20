using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetAllTagAverageReport() : TypedReportQuery, IQuery<AllTagAverageReport>
{
    public int Top { get; init; }

    public ReportInterval Interval { get; init; }
}

internal class GetAllTagAverageReportHandler(IReportRepository repository) : IQueryHandler<GetAllTagAverageReport, AllTagAverageReport>
{
    public async ValueTask<AllTagAverageReport> Handle(GetAllTagAverageReport query, CancellationToken cancellationToken)
    {

        var results = (await repository.GetTopTagAverages(query.AccountId, query.Start, query.End, query.Interval, cancellationToken)).Take(query.Top);

        return new()
        {
            AccountId = query.AccountId,
            Start = query.Start,
            End = query.End,
            ReportType = query.ReportType,
            Tags = results.ToModel(),
        };
    }
}
