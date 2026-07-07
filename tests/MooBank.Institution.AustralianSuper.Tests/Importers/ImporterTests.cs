using System.Text;
using Asm.MooBank.Institution.AustralianSuper.Domain;
using Asm.MooBank.Institution.AustralianSuper.Importers;
using Asm.MooBank.Institution.AustralianSuper.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asm.MooBank.Institution.AustralianSuper.Tests.Importers;

/// <summary>
/// Unit tests for the AustralianSuper <see cref="Importer"/>.
/// Covers contribution amount parsing, payment period parsing and quoted CSV field handling.
/// </summary>
[Trait("Category", "Unit")]
public class ImporterTests
{
    private static readonly Guid InstrumentId = Guid.NewGuid();
    private static readonly Guid InstitutionAccountId = Guid.NewGuid();

    private readonly Mock<ITransactionRawRepository> _rawRepositoryMock = new();
    private readonly Mock<MooBank.Domain.Entities.Transactions.ITransactionRepository> _transactionRepositoryMock = new();
    private readonly List<TransactionRaw> _captured = [];

    public ImporterTests()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _rawRepositoryMock
            .Setup(r => r.AddRange(It.IsAny<IEnumerable<TransactionRaw>>()))
            .Callback<IEnumerable<TransactionRaw>>(_captured.AddRange);
    }

    /// <summary>
    /// Given a contribution row with valid amounts
    /// When the file is imported
    /// Then all four contribution amounts are parsed and stored (regression test for the
    /// short-circuiting validation chain that left every amount as zero).
    /// </summary>
    [Fact]
    public async Task Import_ContributionRow_ParsesContributionAmounts()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(sg: "100.50", employer: "25.00", salarySacrifice: "50.25", member: "10.75", total: "186.50")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(100.50m, raw.SGContributions);
        Assert.Equal(25.00m, raw.EmployerAdditional);
        Assert.Equal(50.25m, raw.SalarySacrifice);
        Assert.Equal(10.75m, raw.MemberAdditional);
        Assert.Equal(186.50m, raw.TotalAmount);

        var transaction = Assert.Single(result.Transactions);
        var extra = Assert.IsType<TransactionExtra>(transaction.Extra);
        Assert.Equal(100.50m, extra.SGContributions);
        Assert.Equal(25.00m, extra.EmployerAdditional);
        Assert.Equal(50.25m, extra.SalarySacrifice);
        Assert.Equal(10.75m, extra.MemberAdditional);
    }

    /// <summary>
    /// Given a contribution row with a payment period of "start/end"
    /// When the file is imported
    /// Then the period start is the earlier date and the period end is the later date.
    /// </summary>
    [Fact]
    public async Task Import_ContributionRow_ParsesPaymentPeriodStartAndEnd()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(period: "2024-06-01/2024-06-14")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(new DateOnly(2024, 6, 1), raw.PaymentPeriodStart);
        Assert.Equal(new DateOnly(2024, 6, 14), raw.PaymentPeriodEnd);
    }

    /// <summary>
    /// Given a contribution row with the payment period written end-first
    /// When the file is imported
    /// Then the dates are normalised so that start is always before end.
    /// </summary>
    [Fact]
    public async Task Import_ContributionRowWithReversedPaymentPeriod_NormalisesOrder()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(period: "2024-06-14/2024-06-01")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(new DateOnly(2024, 6, 1), raw.PaymentPeriodStart);
        Assert.Equal(new DateOnly(2024, 6, 14), raw.PaymentPeriodEnd);
    }

    /// <summary>
    /// Given a contribution row with a malformed payment period
    /// When the file is imported
    /// Then the bad row is skipped with a warning while other rows still import (regression
    /// test for the unguarded ParseExact that aborted the whole import).
    /// </summary>
    [Fact]
    public async Task Import_MalformedPaymentPeriod_SkipsRowAndContinues()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(date: "2024-06-21", period: "not-a-period"),
            Row(date: "2024-06-20", period: "2024-06-01/2024-06-14")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(new DateOnly(2024, 6, 20), raw.Date);
    }

    /// <summary>
    /// Given a row with a quoted description containing commas and escaped quotes
    /// When the file is imported
    /// Then the description is preserved and the columns are not mis-split (regression test
    /// for the hand-rolled CSV parsing that dropped commas inside quoted fields).
    /// </summary>
    [Fact]
    public async Task Import_QuotedFieldWithCommas_ParsesColumnsCorrectly()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(description: "\"Payment, from \"\"Employer, Inc\"\"\"")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal("Payment, from \"Employer, Inc\"", raw.Description);
        Assert.Equal(186.50m, raw.TotalAmount);
    }

    /// <summary>
    /// Given a non-contribution row
    /// When the file is imported
    /// Then the contribution fields remain null and no extra details are attached.
    /// </summary>
    [Fact]
    public async Task Import_NonContributionRow_HasNoContributionDetails()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            Row(category: "INVESTMENT RETURNS", period: "", sg: "", employer: "", salarySacrifice: "", member: "")),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Null(raw.SGContributions);
        Assert.Null(raw.PaymentPeriodStart);
        Assert.Null(raw.PaymentPeriodEnd);

        var transaction = Assert.Single(result.Transactions);
        Assert.Null(transaction.Extra);
    }

    /// <summary>
    /// Given a row that matches an existing raw transaction
    /// When the file is imported
    /// Then the duplicate is skipped.
    /// </summary>
    [Fact]
    public async Task Import_DuplicateRow_IsSkipped()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Employer contribution", new DateOnly(2024, 6, 20), 186.50m)]);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(Row()), TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    private Importer CreateImporter() =>
        new(_rawRepositoryMock.Object, _transactionRepositoryMock.Object, NullLogger<Importer>.Instance);

    /// <summary>
    /// Builds a 24-column CSV row matching the AustralianSuper export format.
    /// </summary>
    private static string Row(
        string date = "2024-06-20",
        string category = "CONTRIBUTIONS",
        string title = "Contribution",
        string description = "Employer contribution",
        string period = "2024-06-01/2024-06-14",
        string sg = "100.50",
        string employer = "25.00",
        string salarySacrifice = "50.25",
        string member = "10.75",
        string total = "186.50")
    {
        string[] columns = new string[24];
        Array.Fill(columns, "");
        columns[0] = date;
        columns[1] = category;
        columns[2] = title;
        columns[3] = description;
        columns[4] = period;
        columns[5] = sg;
        columns[6] = employer;
        columns[7] = salarySacrifice;
        columns[8] = member;
        columns[23] = total;
        return String.Join(",", columns);
    }

    private static MemoryStream ToStream(params string[] rows) =>
        new(Encoding.UTF8.GetBytes(String.Join("\n", ["Header", .. rows])));
}
