namespace Asm.MooBank.Services.DemoData;

/// <summary>
/// The superannuation guarantee rate in force on a given date.
/// </summary>
public static class SuperannuationGuarantee
{
    // Legislated rates, each effective from the start of a financial year. The schedule reached its
    // final step at 12% on 1 July 2025, so the last entry stands until Parliament changes it.
    private static readonly (DateOnly From, decimal Rate)[] Rates =
    [
        (new DateOnly(2013, 7, 1), 0.0925m),
        (new DateOnly(2014, 7, 1), 0.0950m),
        (new DateOnly(2021, 7, 1), 0.1000m),
        (new DateOnly(2022, 7, 1), 0.1050m),
        (new DateOnly(2023, 7, 1), 0.1100m),
        (new DateOnly(2024, 7, 1), 0.1150m),
        (new DateOnly(2025, 7, 1), 0.1200m),
    ];

    public static decimal RateFor(DateOnly date) =>
        Rates.LastOrDefault(r => r.From <= date, Rates[0]).Rate;
}
