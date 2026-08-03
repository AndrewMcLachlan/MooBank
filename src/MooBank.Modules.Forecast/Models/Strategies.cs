namespace Asm.MooBank.Modules.Forecast.Models;

// Income is modelled entirely by planned income items. There is deliberately no income strategy:
// a plan used to carry a fixed monthly figure *and* planned income items, with nothing reconciling
// them, so a salary entered in both places was counted twice. Planned items already express
// fortnightly and monthly schedules with start and end dates, which is everything a pay rise,
// a promotion or a redundancy needs — and, unlike a single figure, they can change over time.

// There is deliberately no mode. Expenses move with income — lower income, less discretionary
// spending — so that is the model, not one option among several. The flat average survives only as
// the degenerate case, used when there is not enough signal to fit a slope, and it is reported as a
// fallback rather than chosen. The mode this replaces was named "HistoricalAverageByTag" and did
// nothing by tag: it averaged every debit.

public sealed record OutgoingStrategy
{
    public int Version { get; init; } = 1;
    public int LookbackMonths { get; init; } = 24;

    // TODO: not yet honoured by ForecastEngine.
    public IEnumerable<int>? ExcludeTagIds { get; init; }

    // TODO: not yet honoured by ForecastEngine.
    public decimal? ExcludeAboveAmount { get; init; }

    // TODO: not yet honoured by ForecastEngine.
    public SeasonalitySettings? Seasonality { get; init; }

    public IncomeCorrelatedSettings? IncomeCorrelated { get; init; }
}

// There is deliberately no R-squared threshold. Household spending is noisy, so a real relationship
// between income and spending rarely reaches a high correlation over a year or two of monthly
// points -- and rejecting it left the forecast on a flat line, which cannot answer what happens when
// income changes. A fit that explains some of the variation beats one that explains none.
public sealed record IncomeCorrelatedSettings
{
    public int MinDataPoints { get; init; } = 6;
}

public sealed record SeasonalitySettings
{
    public bool Enabled { get; init; }
}

// TODO: not yet honoured by ForecastEngine — persisted but never read when forecasting.
public sealed record Assumptions
{
    public int Version { get; init; } = 1;
    public decimal? InflationRateAnnual { get; init; }
    public bool ApplyInflationToBaseline { get; init; }
    public bool ApplyInflationToPlanned { get; init; }
    public decimal SafetyBuffer { get; init; }
}
