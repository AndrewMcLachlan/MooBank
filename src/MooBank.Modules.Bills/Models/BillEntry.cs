using System.ComponentModel;

namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// A bill as read off a supplier's invoice.
/// </summary>
/// <remarks>
/// Deliberately does not derive from <see cref="BillBase"/>. Cost and Total are computed columns,
/// so any value supplied for them is dropped on insert. Leaving them off this contract stops a
/// caller supplying the figures printed on the invoice and believing they were stored.
/// </remarks>
public record BillEntry
{
    [Description("The name of the utility account the bill belongs to, exactly as returned by get-bill-accounts.")]
    public required string AccountName { get; set; }

    [Description("The invoice number printed on the bill. Must be 15 characters or fewer.")]
    public string? InvoiceNumber { get; set; }

    [Description("The date the bill was issued.")]
    public required DateOnly IssueDate { get; set; }

    [Description("The meter reading at the end of the billing period, where the bill shows one.")]
    public int? CurrentReading { get; set; }

    [Description("The meter reading at the start of the billing period, where the bill shows one.")]
    public int? PreviousReading { get; set; }

    [Description("Whether the charges on the bill are inclusive of GST.")]
    public bool? CostsIncludeGST { get; set; }

    [Description("The billing periods covered. A bill with a tariff change part-way through covers more than one period.")]
    public IEnumerable<BillEntryPeriod> Periods { get; set; } = [];

    [Description("Any discounts applied to the bill.")]
    public IEnumerable<Discount> Discounts { get; set; } = [];
}

/// <summary>
/// A single billing period within a <see cref="BillEntry"/>.
/// </summary>
/// <remarks>
/// Carries only the fields that are actually persisted; Days, DaysInclusive and Cost are all
/// derived by the database from the values below.
/// </remarks>
public record BillEntryPeriod
{
    [Description("The first day of the billing period.")]
    public DateOnly PeriodStart { get; set; }

    [Description("The last day of the billing period.")]
    public DateOnly PeriodEnd { get; set; }

    [Description("The price charged per unit of usage.")]
    public decimal PricePerUnit { get; set; }

    [Description("The total units consumed during the period.")]
    public decimal TotalUsage { get; set; }

    [Description("The daily supply or service charge.")]
    public decimal ChargePerDay { get; set; }
}

public static class BillEntryExtensions
{
    /// <summary>
    /// Convert to the shape the import command consumes.
    /// </summary>
    public static ImportBill ToImportBill(this BillEntry entry) =>
        new()
        {
            AccountName = entry.AccountName,
            InvoiceNumber = entry.InvoiceNumber,
            IssueDate = entry.IssueDate,
            CurrentReading = entry.CurrentReading,
            PreviousReading = entry.PreviousReading,
            CostsIncludeGST = entry.CostsIncludeGST,
            Periods = entry.Periods.Select(p => new Period
            {
                PeriodStart = p.PeriodStart.ToDateTime(TimeOnly.MinValue),
                PeriodEnd = p.PeriodEnd.ToDateTime(TimeOnly.MinValue),
                PricePerUnit = p.PricePerUnit,
                TotalUsage = p.TotalUsage,
                ChargePerDay = p.ChargePerDay,
            }).ToList(),
            Discounts = entry.Discounts.ToList(),
        };

    public static IEnumerable<ImportBill> ToImportBill(this IEnumerable<BillEntry> entries) =>
        entries.Select(e => e.ToImportBill());
}
