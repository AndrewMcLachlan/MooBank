using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Bills.Models;

public record Period
{
    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int? DaysInclusive { get; set; }

    public int? Days { get; set; }

    public IEnumerable<Usage> Usages { get; set; } = [];

    public IEnumerable<ServiceCharge> ServiceCharges { get; set; } = [];
}

public record Usage
{
    public UsageType UsageType { get; set; } = UsageType.Consumption;

    public decimal PricePerUnit { get; set; }

    public decimal TotalUsage { get; set; }

    /// <summary>
    /// Negative for export, which the retailer credits.
    /// </summary>
    public decimal? Cost { get; set; }
}

public record ServiceCharge
{
    public int ChargeTypeId { get; set; }

    public string? ChargeTypeName { get; set; }

    public decimal ChargePerDay { get; set; }
}

public static class PeriodExtensions
{
    public static Period ToModel(this Domain.Entities.Utility.Period period) =>
        new()
        {
            ServiceCharges = period.ServiceCharges.Select(sc => sc.ToModel()).ToList(),
            Usages = period.Usages.Select(u => u.ToModel()).ToList(),
            Days = period.Days,
            DaysInclusive = period.DaysInclusive,
            PeriodStart = period.PeriodStart,
            PeriodEnd = period.PeriodEnd,
        };

    public static IEnumerable<Period> ToModel(this IEnumerable<Domain.Entities.Utility.Period> periods) =>
        periods.Select(p => p.ToModel());

    public static ServiceCharge ToModel(this Domain.Entities.Utility.ServiceCharge serviceCharge) =>
        new()
        {
            ChargeTypeId = serviceCharge.ChargeTypeId,
            ChargeTypeName = serviceCharge.ChargeType?.Name,
            ChargePerDay = serviceCharge.ChargePerDay,
        };

    public static Usage ToModel(this Domain.Entities.Utility.Usage usage) =>
        new()
        {
            UsageType = usage.UsageType,
            PricePerUnit = usage.PricePerUnit,
            TotalUsage = usage.TotalUsage,
            Cost = usage.Cost,
        };
}
