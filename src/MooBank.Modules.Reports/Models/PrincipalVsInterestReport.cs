namespace Asm.MooBank.Modules.Reports.Models;

public record PrincipalVsInterestReport : ReportBase
{
    public int? InterestTagId { get; init; }

    public string? InterestTagName { get; init; }

    public required IEnumerable<TrendPoint> Interest { get; init; } = [];

    public required IEnumerable<TrendPoint> Principal { get; init; } = [];

    public required decimal InterestTotal { get; init; }

    public required decimal PrincipalTotal { get; init; }
}
