using System.Text;
using Asm.MooBank.Institution.Macquarie.Domain;
using Asm.MooBank.Institution.Macquarie.Importers;
using Microsoft.Extensions.Logging.Abstractions;
using DomainTransactionType = Asm.MooBank.TransactionType;

namespace Asm.MooBank.Institution.Macquarie.Tests.Importers;

/// <summary>
/// Unit tests for the <see cref="MacquarieImporter"/>.
/// Covers validation skips, credit/debit sign mapping, detail/sub-type mapping, duplicate detection
/// (exact, pending-balance-update and genuine-new fall-through), end-balance seeding, per-date
/// sequence numbering and reprocessing.
/// </summary>
[Trait("Category", "Unit")]
public class MacquarieImporterTests
{
    private const string Header = "Transaction Date,Details,Account,Category,Subcategory,Tags,Notes,Debit,Credit,Balance,Original Description";

    private static readonly Guid InstrumentId = Guid.NewGuid();
    private static readonly Guid InstitutionAccountId = Guid.NewGuid();

    private readonly Mock<ITransactionRawRepository> _rawRepositoryMock = new();
    private readonly Mock<MooBank.Domain.Entities.Transactions.ITransactionRepository> _transactionRepositoryMock = new();
    private readonly List<TransactionRaw> _captured = [];

    public MacquarieImporterTests()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _rawRepositoryMock
            .Setup(r => r.AddRange(It.IsAny<IEnumerable<TransactionRaw>>()))
            .Callback<IEnumerable<TransactionRaw>>(_captured.AddRange);
    }

    #region Sign mapping and details

    /// <summary>
    /// Given a credit row
    /// When the file is imported
    /// Then a positive-amount credit transaction is created and the end balance is seeded.
    /// </summary>
    [Fact]
    public async Task Import_CreditRow_CreatesPositiveTransaction()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Salary,Account,Income,Wages,,,,1500.00,5000.00,Salary"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(1500.00m, raw.Transaction.Amount);
        Assert.Equal(DomainTransactionType.Credit, raw.Transaction.TransactionType);
        Assert.Equal(1500.00m, raw.Credit);
        Assert.Equal(5000.00m, result.EndBalance);
    }

    /// <summary>
    /// Given a debit row
    /// When the file is imported
    /// Then a negative-amount debit transaction is created.
    /// </summary>
    [Fact]
    public async Task Import_DebitRow_CreatesNegativeTransaction()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Groceries,Account,Food,Supermarket,,,45.50,,4954.50,Woolworths"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(-45.50m, raw.Transaction.Amount);
        Assert.Equal(DomainTransactionType.Debit, raw.Transaction.TransactionType);
    }

    /// <summary>
    /// Given a row whose Details is "Payment"
    /// When the file is imported
    /// Then the description is expanded to "Payment - {Subcategory}".
    /// </summary>
    [Fact]
    public async Task Import_PaymentDetails_PrefixesSubcategory()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Payment,Account,Bills,Electricity,,,80.00,,4920.00,BPAY"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal("Payment - Electricity", raw.Transaction.Description);
    }

    /// <summary>
    /// Given a row whose Subcategory is "Transfers"
    /// When the file is imported
    /// Then the transaction sub-type is set to Transfer.
    /// </summary>
    [Fact]
    public async Task Import_TransfersSubcategory_SetsTransferSubType()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Move money,Account,Transfer,Transfers,,,100.00,,4900.00,Transfer"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(MooBank.TransactionSubType.Transfer, raw.Transaction.TransactionSubType);
    }

    #endregion

    #region Validation skips

    /// <summary>
    /// Given a row with neither a credit nor a debit amount
    /// When the file is imported
    /// Then the row is skipped.
    /// </summary>
    [Fact]
    public async Task Import_NeitherCreditNorDebit_SkipsRow()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,No amount,Account,Cat,Sub,,,,,4900.00,Desc"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given a row with BOTH a credit and a debit amount
    /// When the file is imported
    /// Then the row is skipped (credit/debit are mutually exclusive).
    /// </summary>
    [Fact]
    public async Task Import_BothCreditAndDebit_SkipsRow()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Both amounts,Account,Cat,Sub,,,10.00,20.00,4900.00,Desc"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given a row with no Details
    /// When the file is imported
    /// Then the row is skipped.
    /// </summary>
    [Fact]
    public async Task Import_MissingDetails_SkipsRow()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,,Account,Cat,Sub,,,45.00,,4900.00,Desc"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given a row with an unparseable date
    /// When the file is imported
    /// Then the row is skipped.
    /// </summary>
    [Fact]
    public async Task Import_InvalidDate_SkipsRow()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "not-a-date,Groceries,Account,Cat,Sub,,,45.00,,4900.00,Desc"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given a row with an empty Balance
    /// When the file is imported
    /// Then it is treated as a pending transaction and skipped.
    /// </summary>
    [Fact]
    public async Task Import_EmptyBalance_SkipsAsPending()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Pending,Account,Cat,Sub,,,45.00,,,Desc"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
        Assert.Null(result.EndBalance);
    }

    /// <summary>
    /// Given a file with no data rows
    /// When the file is imported
    /// Then the result is returned without an end balance.
    /// </summary>
    [Fact]
    public async Task Import_EmptyFile_ReturnsNoEndBalance()
    {
        var importer = CreateImporter();

        var result = await importer.Import(InstrumentId, InstitutionAccountId, ToStream(), TestContext.Current.CancellationToken);

        Assert.Empty(result.Transactions);
        Assert.Null(result.EndBalance);
    }

    #endregion

    #region Duplicate detection

    /// <summary>
    /// Given an existing transaction with identical details, date, amount AND balance
    /// When the file is imported
    /// Then the row is skipped as an exact duplicate.
    /// </summary>
    [Fact]
    public async Task Import_ExactDuplicate_IsSkipped()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Groceries", new DateOnly(2024, 6, 20), 0m, 45.50m, 4954.50m)]);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Groceries,Account,Food,Supermarket,,,45.50,,4954.50,Woolworths"),
            TestContext.Current.CancellationToken);

        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given an existing pending transaction matching on details, date and amount but not balance
    /// When the file is imported
    /// Then the pending row's balance is updated and the row is skipped (not re-inserted).
    /// </summary>
    [Fact]
    public async Task Import_PendingMatch_UpdatesBalanceAndSkips()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Groceries", new DateOnly(2024, 6, 20), 0m, 45.50m, null)]);

        var pending = new TransactionRaw(Guid.NewGuid()) { Balance = null };
        _rawRepositoryMock
            .Setup(r => r.GetZeroBalance(InstrumentId, "Groceries", new DateOnly(2024, 6, 20), 45.50m, 0m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pending);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Groceries,Account,Food,Supermarket,,,45.50,,4954.50,Woolworths"),
            TestContext.Current.CancellationToken);

        Assert.Equal(4954.50m, pending.Balance);
        Assert.Empty(_captured);
    }

    /// <summary>
    /// Given an amount/details/date match but NO corresponding pending (zero-balance) row
    /// When the file is imported
    /// Then the transaction is genuinely new and is inserted (fall-through path).
    /// </summary>
    [Fact]
    public async Task Import_AmountMatchButNoPendingRow_InsertsAsNew()
    {
        _rawRepositoryMock
            .Setup(r => r.GetSummaries(InstrumentId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TransactionRawSummary("Groceries", new DateOnly(2024, 6, 20), 0m, 45.50m, 9999.00m)]);

        _rawRepositoryMock
            .Setup(r => r.GetZeroBalance(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionRaw?)null);

        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,Groceries,Account,Food,Supermarket,,,45.50,,4954.50,Woolworths"),
            TestContext.Current.CancellationToken);

        var raw = Assert.Single(_captured);
        Assert.Equal(-45.50m, raw.Transaction.Amount);
    }

    #endregion

    #region Sequence numbering

    /// <summary>
    /// Given multiple rows on the same date followed by a row on a different date
    /// When the file is imported
    /// Then the sequence number increments within a date and resets on a date change.
    /// NOTE: the importer's comment says it "subtracts from 59 to preserve order"; the code actually
    /// resets to 1 then pre-increments, so the first row of each date is numbered 2. This test asserts
    /// the ACTUAL persisted behaviour — existing rows encode it, so it must not be "corrected".
    /// </summary>
    [Fact]
    public async Task Import_SequenceNumber_IncrementsWithinDateAndResetsOnChange()
    {
        var importer = CreateImporter();

        await importer.Import(InstrumentId, InstitutionAccountId, ToStream(
            "20 Jun 2024,First,Account,Cat,Sub,,,10.00,,4990.00,d1a",
            "20 Jun 2024,Second,Account,Cat,Sub,,,10.00,,4980.00,d1b",
            "19 Jun 2024,Third,Account,Cat,Sub,,,10.00,,4970.00,d2a"),
            TestContext.Current.CancellationToken);

        Assert.Equal(3, _captured.Count);
        Assert.Equal(2, _captured[0].SequenceNumber);
        Assert.Equal(3, _captured[1].SequenceNumber);
        Assert.Equal(2, _captured[2].SequenceNumber);
    }

    #endregion

    #region Reprocess

    /// <summary>
    /// Given stored raw transactions whose transaction ids are still present
    /// When Reprocess is called
    /// Then each transaction's type, amount and description are recomputed from the raw data.
    /// </summary>
    [Fact]
    public async Task Reprocess_RecomputesTransactionFromRaw()
    {
        var transactionId = Guid.NewGuid();
        var transaction = MooBank.Domain.Entities.Transactions.Transaction.Create(
            InstrumentId, null, 1m, "Stale", new DateTime(2024, 1, 1), null, "Macquarie Import", InstitutionAccountId,
            transactionType: DomainTransactionType.Credit);

        var raw = new TransactionRaw(Guid.NewGuid())
        {
            TransactionId = transactionId,
            Date = new DateOnly(2024, 6, 20),
            Details = "Groceries",
            Subcategory = "Supermarket",
            Debit = 45.50m,
            Credit = 0m,
            Transaction = transaction,
        };

        _transactionRepositoryMock
            .Setup(r => r.GetTransactionIds(InstrumentId, InstitutionAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([transactionId]);
        _rawRepositoryMock
            .Setup(r => r.GetAll(InstrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([raw]);

        var importer = CreateImporter();

        await importer.Reprocess(InstrumentId, InstitutionAccountId, TestContext.Current.CancellationToken);

        Assert.Equal(DomainTransactionType.Debit, transaction.TransactionType);
        Assert.Equal(-45.50m, transaction.Amount);
        Assert.Equal("Groceries", transaction.Description);
    }

    #endregion

    private MacquarieImporter CreateImporter() =>
        new(_rawRepositoryMock.Object, _transactionRepositoryMock.Object, NullLogger<MacquarieImporter>.Instance);

    private static MemoryStream ToStream(params string[] rows) =>
        new(Encoding.UTF8.GetBytes(String.Join("\n", [Header, .. rows])));
}
