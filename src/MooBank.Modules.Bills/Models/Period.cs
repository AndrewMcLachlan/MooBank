namespace Asm.MooBank.Modules.Bills.Models;

public record Period
{
    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int? DaysInclusive { get; set; }

    public int? Days { get; set; }

    public decimal PricePerUnit { get; set; }

    public decimal TotalUsage { get; set; }

    public decimal? Cost { get; set; }

    public IEnumerable<ServiceCharge> ServiceCharges { get; set; } = [];
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
            Cost = period.Usage?.Cost,
            Days = period.Days,
            DaysInclusive = period.DaysInclusive,
            PeriodStart = period.PeriodStart,
            PeriodEnd = period.PeriodEnd,
            PricePerUnit = period.Usage?.PricePerUnit ?? 0,
            TotalUsage = period.Usage?.TotalUsage ?? 0,
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
}
