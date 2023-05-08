using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetTagTrendReport : TypedReportQuery, IQuery<TagTrendReport>
{
    public required int TagId { get; init; }

    public TagTrendReportSettings Settings { get; init; } = new TagTrendReportSettings();
}

public record TagTrendReportSettings
{
    public bool ApplySmoothing { get; init; }
}