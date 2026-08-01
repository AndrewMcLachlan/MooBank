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

    /// <summary>
    /// How far either side of a planned item's own dates a payment carrying its tag still counts as
    /// that item's, in months.
    /// </summary>
    /// <remarks>
    /// A slippage allowance for a bill paid late or early, not a way to model spending genuinely
    /// spread over a period — that is what a flexible window is for. It applies to every item on the
    /// plan at once, so widening it to cover one long job loosens matching for all the rest.
    /// </remarks>
    public int MatchWindowMonths { get; init; } = 1;

    // TODO: not yet honoured by ForecastEngine.
    public IEnumerable<int>? ExcludeTagIds { get; init; }

    // TODO: not yet honoured by ForecastEngine.
    public decimal? ExcludeAboveAmount { get; init; }

    // TODO: not yet honoured by ForecastEngine.
    public SeasonalitySettings? Seasonality { get; init; }

    public IncomeCorrelatedSettings? IncomeCorrelated { get; init; }
}

public sealed record IncomeCorrelatedSettings
{
    public decimal RSquaredThreshold { get; init; } = 0.5m;
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
