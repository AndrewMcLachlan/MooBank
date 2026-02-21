namespace Asm.MooBank.Modules.Forecast.Models;

public sealed record IncomeStrategy
{
    public int Version { get; init; } = 1;
    public string Mode { get; init; } = "ManualRecurring";
    public ManualRecurringIncome? ManualRecurring { get; init; }
    public IEnumerable<ManualAdjustment>? ManualAdjustments { get; init; }
    public HistoricalIncomeSettings? Historical { get; init; }
}

public sealed record ManualRecurringIncome
{
    public decimal Amount { get; init; }
    public string Frequency { get; init; } = "Monthly";
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}

public sealed record ManualAdjustment
{
    public DateOnly Date { get; init; }
    public decimal DeltaAmount { get; init; }
}

public sealed record HistoricalIncomeSettings
{
    public int LookbackMonths { get; init; } = 12;
    public IEnumerable<int>? IncludeTagIds { get; init; }
    public IEnumerable<int>? ExcludeTagIds { get; init; }
    public bool ExcludeTransfers { get; init; } = true;
    public bool ExcludeOffsets { get; init; }
}

public sealed record OutgoingStrategy
{
    public int Version { get; init; } = 1;
    public string Mode { get; init; } = "HistoricalAverageByTag";
    public int LookbackMonths { get; init; } = 12;
    public IEnumerable<int>? ExcludeTagIds { get; init; }
    public decimal? ExcludeAboveAmount { get; init; }
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

public sealed record Assumptions
{
    public int Version { get; init; } = 1;
    public decimal? InflationRateAnnual { get; init; }
    public bool ApplyInflationToBaseline { get; init; }
    public bool ApplyInflationToPlanned { get; init; }
    public decimal SafetyBuffer { get; init; }
}
