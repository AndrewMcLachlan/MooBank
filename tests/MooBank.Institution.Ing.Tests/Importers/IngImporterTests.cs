using System.Text;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Institution.Ing.Domain;
using Asm.MooBank.Institution.Ing.Importers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asm.MooBank.Institution.Ing.Tests.Importers;

/// <summary>
/// Unit tests for the <see cref="IngImporter"/>.
/// Covers quoted CSV field handling, empty file handling and receipt-based duplicate detection.
/// </summary>
[Trait("Category", "Unit")]
public class IngImporterTests
{
    private const string Header = "Date,Description,Credit,Debit,Balance";

    private static readonly Guid InstrumentId = Guid.NewGuid();
    private static readonly Guid InstitutionAccountId = Guid.NewGuid();

    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ITransactionRawRepository> _rawRepositoryMock = new();
    private readonly Mock<MooBank.Domain.Entities.Transactions.ITransactionRepository> _transactionRepositoryMock = new();
    private readonly List<TransactionRaw> _captured = [];

    public IngImporterTests()
    {
        _userRepositoryMock
            .Setup(r => r.GetByCard(It.IsAny<short>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _rawRepositoryMock
            .Setup(r => r.GetSummaries(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _rawRepositoryMock
            .Setup(r => r.AddRange(It.IsAny<IEnumerable<TransactionRaw>>()))
            .Callback<IEnumerable<TransactionRaw>>(_captured.AddRange);
    }

    /// <summary>
    /// Given a quoted description containing commas and escaped quotes
    /// When the file is imported
    /// Then the description is preserved intact (regression test for the hand-rolled CSV
    /// parsing that dropped commas inside quoted fields).
    /// </summary>
    [Fact]
    public async Task Import_QuotedDescriptionWithCommas_PreservesDescription()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20/06/2024,\"TRANSFER, FROM \"\"SAVINGS, ACCOUNT\"\"\",,50.00,1000.00"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal("TRANSFER, FROM \"SAVINGS, ACCOUNT\"", raw.Description);
        Assert.Equal(50.00m, raw.Debit);
        Assert.Equal(1000.00m, result.EndBalance);
    }

    /// <summary>
    /// Given a file with no data rows
    /// When the file is imported
    /// Then the result is returned without an end balance instead of throwing (regression
    /// test for the unguarded endBalance!.Value).
    /// </summary>
    [Fact]
    public async Task Import_EmptyFile_ReturnsResultWithoutEndBalance()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(), TestContext.Current.CancellationToken);

        Assert.Empty(result.Transactions);
        Assert.Null(result.EndBalance);
    }

    /// <summary>
    /// Given a file where all rows are invalid
    /// When the file is imported
    /// Then the rows are skipped and the result is returned without an end balance.
    /// </summary>
    [Fact]
    public async Task Import_AllRowsInvalid_ReturnsResultWithoutEndBalance()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "garbage,No amounts,,,",
            "not-a-date,Description,,50.00,1000.00"),
            TestContext.Current.CancellationToken);

        Assert.Empty(result.Transactions);
        Assert.Null(result.EndBalance);
    }

    /// <summary>
    /// Given an existing transaction whose description cannot be parsed (no receipt number)
    /// When a different, also unparseable, transaction with the same date and amount is imported
    /// Then it is NOT treated as a duplicate (regression test for null == null receipt matching).
    /// </summary>
    [Fact]
    public async Task Import_UnparseableDescriptions_DoesNotMatchOnNullReceiptNumbers()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Mystery Payment A", new DateOnly(2024, 6, 20), 0m, 50.00m)]);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20/06/2024,Mystery Payment B,,50.00,1000.00"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal("Mystery Payment B", raw.Description);
    }

    /// <summary>
    /// Given an existing transaction with the same receipt number, date and amount
    /// When the file is imported
    /// Then the transaction is treated as a duplicate and skipped.
    /// </summary>
    [Fact]
    public async Task Import_MatchingReceiptNumbers_IsSkippedAsDuplicate()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Power Co - Direct Debit - Receipt 123456 Bill", new DateOnly(2024, 6, 20), 0m, 50.00m)]);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20/06/2024,Power Company - Direct Debit - Receipt 123456 Electricity Bill,,50.00,1000.00"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given an identical description on the same date with the same amount
    /// When the file is imported
    /// Then the transaction is treated as a duplicate and skipped.
    /// </summary>
    [Fact]
    public async Task Import_IdenticalDescription_IsSkippedAsDuplicate()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Mystery Payment A", new DateOnly(2024, 6, 20), 0m, 50.00m)]);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20/06/2024,Mystery Payment A,,50.00,1000.00"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    private IngImporter CreateImporter() =>
        new(_userRepositoryMock.Object, _rawRepositoryMock.Object, _transactionRepositoryMock.Object, NullLogger<IngImporter>.Instance);

    private static MemoryStream ToStream(params string[] rows) =>
        new(Encoding.UTF8.GetBytes(String.Join("\n", [Header, .. rows])));
}
