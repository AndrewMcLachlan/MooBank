namespace Asm.MooBank.Modules.Bills.Models;

public record Period
{
    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int DaysInclusive { get; set; }

    public int Days { get; set; }

    public decimal PricePerUnit { get; set; }

    public decimal TotalUsage { get; set; }

    public decimal Cost { get; set; }

    public decimal ChargePerDay { get; set; }
}

public static class PeriodExtensions
{
    public static Period ToModel(this Domain.Entities.Utility.Period period) =>
        new()
        {
            ChargePerDay = period.ServiceCharge.ChargePerDay,
            Cost = period.Usage.Cost,
            Days = period.Days,
            DaysInclusive = period.DaysInclusive,
            PeriodStart = period.PeriodStart,
            PeriodEnd = period.PeriodEnd,
            PricePerUnit = period.Usage.PricePerUnit,
            TotalUsage = period.Usage.TotalUsage,
        };

    public static IEnumerable<Period> ToModel(this IEnumerable<Domain.Entities.Utility.Period> periods) =>
        periods.Select(p => p.ToModel());
}
