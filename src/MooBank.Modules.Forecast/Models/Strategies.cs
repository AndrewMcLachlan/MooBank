namespace Asm.MooBank.Modules.Forecast.Models;

// Income is modelled entirely by planned income items. There is deliberately no income strategy:
// a plan used to carry a fixed monthly figure *and* planned income items, with nothing reconciling
// them, so a salary entered in both places was counted twice. Planned items already express
// fortnightly and monthly schedules with start and end dates, which is everything a pay rise,
// a promotion or a redundancy needs — and, unlike a single figure, they can change over time.

public sealed record OutgoingStrategy
{
    public int Version { get; init; } = 1;
    public string Mode { get; init; } = "HistoricalAverageByTag";
    public int LookbackMonths { get; init; } = 12;

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
