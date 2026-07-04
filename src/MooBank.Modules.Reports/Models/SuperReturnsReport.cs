namespace Asm.MooBank.Modules.Reports.Models;

public record SuperReturnsReport : ReportBase
{
    public required IEnumerable<SuperReturnsYear> Years { get; init; } = [];
}

public record SuperReturnsYear
{
    public required int FinancialYear { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required decimal OpeningBalance { get; init; }

    public required decimal ClosingBalance { get; init; }

    public required decimal Contributions { get; init; }

    public required decimal Return { get; init; }

    public decimal? ReturnPercent { get; init; }
}
