namespace Asm.MooBank.Modules.Reports.Models;

public record SuperContributionsReport : ReportBase
{
    public int? EmployerTagId { get; init; }

    public string? EmployerTagName { get; init; }

    public int? PersonalTagId { get; init; }

    public string? PersonalTagName { get; init; }

    public required IEnumerable<TrendPoint> Employer { get; init; } = [];

    public required IEnumerable<TrendPoint> Personal { get; init; } = [];

    public required decimal EmployerTotal { get; init; }

    public required decimal PersonalTotal { get; init; }
}
