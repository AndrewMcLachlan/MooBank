#nullable enable
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Tests.Models;

/// <summary>
/// Unit tests for mapping a <see cref="BillEntry"/> onto the shape the import command consumes.
/// </summary>
[Trait("Category", "Unit")]
public class BillEntryTests
{
    /// <summary>
    /// Given a bill entry with account, invoice and reading details
    /// When it is converted to an import bill
    /// Then those details should be carried across unchanged
    /// </summary>
    [Fact]
    public void ToImportBill_BillDetails_AreCarriedAcross()
    {
        // Arrange
        var entry = new BillEntry
        {
            AccountName = "AGL Electricity",
            InvoiceNumber = "INV001",
            IssueDate = new DateOnly(2026, 3, 14),
            CurrentReading = 5400,
            PreviousReading = 5100,
            CostsIncludeGST = true,
        };

        // Act
        var importBill = entry.ToImportBill();

        // Assert
        Assert.Equal("AGL Electricity", importBill.AccountName);
        Assert.Equal("INV001", importBill.InvoiceNumber);
        Assert.Equal(new DateOnly(2026, 3, 14), importBill.IssueDate);
        Assert.Equal(5400, importBill.CurrentReading);
        Assert.Equal(5100, importBill.PreviousReading);
        Assert.True(importBill.CostsIncludeGST);
    }

    /// <summary>
    /// Given any bill entry
    /// When it is converted to an import bill
    /// Then Cost and Total should be left unset
    /// </summary>
    /// <remarks>
    /// Both are computed columns, so a value here would be silently dropped on insert. The entry
    /// contract omits them precisely so a caller cannot supply the figures printed on an invoice
    /// and believe they were stored; this guards that the mapping does not reintroduce them.
    /// </remarks>
    [Fact]
    public void ToImportBill_ComputedFields_AreNotPopulated()
    {
        // Arrange
        var entry = new BillEntry
        {
            AccountName = "AGL Electricity",
            IssueDate = new DateOnly(2026, 3, 14),
            CurrentReading = 5400,
            PreviousReading = 5100,
        };

        // Act
        var importBill = entry.ToImportBill();

        // Assert
        Assert.Null(importBill.Cost);
        Assert.Null(importBill.Total);
    }

    /// <summary>
    /// Given a bill entry with billing periods
    /// When it is converted to an import bill
    /// Then each period's dates should become midnight on the same day and its charges carried across
    /// </summary>
    [Fact]
    public void ToImportBill_Periods_AreMappedToMidnightOnTheSameDay()
    {
        // Arrange
        var entry = new BillEntry
        {
            AccountName = "AGL Electricity",
            IssueDate = new DateOnly(2026, 3, 14),
            Periods =
            [
                new BillEntryPeriod
                {
                    PeriodStart = new DateOnly(2026, 1, 1),
                    PeriodEnd = new DateOnly(2026, 1, 31),
                    PricePerUnit = 0.28m,
                    TotalUsage = 412.5m,
                    ServiceCharges = [new BillEntryServiceCharge { ChargeTypeId = 1, ChargePerDay = 1.15m }],
                },
            ],
        };

        // Act
        var importBill = entry.ToImportBill();

        // Assert
        var period = Assert.Single(importBill.Periods);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0), period.PeriodStart);
        Assert.Equal(new DateTime(2026, 1, 31, 0, 0, 0), period.PeriodEnd);
        Assert.Equal(0.28m, period.PricePerUnit);
        Assert.Equal(412.5m, period.TotalUsage);
        var serviceCharge = Assert.Single(period.ServiceCharges);
        Assert.Equal(1, serviceCharge.ChargeTypeId);
        Assert.Equal(1.15m, serviceCharge.ChargePerDay);
    }

    /// <summary>
    /// Given a bill entry with discounts
    /// When it is converted to an import bill
    /// Then the discounts should be carried across
    /// </summary>
    [Fact]
    public void ToImportBill_Discounts_AreCarriedAcross()
    {
        // Arrange
        var entry = new BillEntry
        {
            AccountName = "AGL Electricity",
            IssueDate = new DateOnly(2026, 3, 14),
            Discounts =
            [
                new Discount { DiscountPercent = 12, Reason = "Pay on time" },
                new Discount { DiscountAmount = 25m, Reason = "Loyalty" },
            ],
        };

        // Act
        var importBill = entry.ToImportBill();

        // Assert
        Assert.Collection(
            importBill.Discounts,
            d => Assert.Equal((byte)12, d.DiscountPercent),
            d => Assert.Equal(25m, d.DiscountAmount));
    }

    /// <summary>
    /// Given several bill entries
    /// When the collection is converted
    /// Then every entry should be mapped
    /// </summary>
    [Fact]
    public void ToImportBill_Collection_MapsEveryEntry()
    {
        // Arrange
        BillEntry[] entries =
        [
            new() { AccountName = "AGL Electricity", IssueDate = new DateOnly(2026, 1, 14) },
            new() { AccountName = "Origin Gas", IssueDate = new DateOnly(2026, 2, 14) },
        ];

        // Act
        var importBills = entries.ToImportBill().ToList();

        // Assert
        Assert.Equal(2, importBills.Count);
        Assert.Equal("AGL Electricity", importBills[0].AccountName);
        Assert.Equal("Origin Gas", importBills[1].AccountName);
    }
}
