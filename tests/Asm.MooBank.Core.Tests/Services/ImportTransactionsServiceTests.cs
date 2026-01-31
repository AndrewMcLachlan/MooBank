using Asm.Domain;
using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="ImportTransactionsService"/> class.
/// Tests verify import orchestration, rule application, and error handling.
/// </summary>
public class ImportTransactionsServiceTests
{
    private readonly Mock<IInstrumentRepository> _instrumentRepositoryMock;
    private readonly Mock<IRuleRepository> _ruleRepositoryMock;
    private readonly Mock<IImporterFactory> _importerFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IImporter> _importerMock;
    private readonly ILogger<ImportTransactionsService> _logger;
    private readonly TestEntities _entities = new();

    public ImportTransactionsServiceTests()
    {
        _instrumentRepositoryMock = new Mock<IInstrumentRepository>();
        _ruleRepositoryMock = new Mock<IRuleRepository>();
        _importerFactoryMock = new Mock<IImporterFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _importerMock = new Mock<IImporter>();
        _logger = NullLoggerFactory.Instance.CreateLogger<ImportTransactionsService>();

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _ruleRepositoryMock.Setup(r => r.GetForInstrument(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    #region Successful Import

    /// <summary>
    /// Given a valid work item
    /// When Import is called
    /// Then the importer's Import method should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithValidWorkItem_CallsImporterImport()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        SetupSuccessfulImport(workItem, instrument, []);
        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        _importerMock.Verify(
            i => i.Import(workItem.InstrumentId, workItem.AccountId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Given a valid work item
    /// When Import is called
    /// Then changes should be saved to the database
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithValidWorkItem_SavesChanges()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        SetupSuccessfulImport(workItem, instrument, []);
        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a valid work item with file data
    /// When Import is called
    /// Then the file data should be passed to the importer as a stream
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithFileData_PassesDataAsStream()
    {
        // Arrange
        var fileData = new byte[] { 1, 2, 3, 4, 5 };
        var workItem = CreateWorkItem(fileData);
        var instrument = CreateInstrument(workItem.InstrumentId);
        byte[]? capturedStreamData = null;

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);

        _importerMock
            .Setup(i => i.Import(workItem.InstrumentId, workItem.AccountId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid?, Stream, CancellationToken>((_, _, stream, _) =>
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                capturedStreamData = ms.ToArray();
            })
            .ReturnsAsync(new TransactionImportResult([]));

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Equal(fileData, capturedStreamData);
    }

    #endregion

    #region Rule Application

    /// <summary>
    /// Given imported transactions and matching rules
    /// When Import is called
    /// Then rules should be applied to transactions
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithMatchingRules_AppliesTagsToTransactions()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var tag = _entities.CreateTag(1, "Groceries");
        var transaction = CreateTransaction("Costco Purchase");
        var rule = CreateRule(1, "Costco", workItem.InstrumentId, [tag]);

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
    }

    /// <summary>
    /// Given imported transactions with non-matching rules
    /// When Import is called
    /// Then no tags should be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithNonMatchingRules_DoesNotApplyTags()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var tag = _entities.CreateTag(1, "Groceries");
        var transaction = CreateTransaction("Amazon Order");
        var rule = CreateRule(1, "Costco", workItem.InstrumentId, [tag]);

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Empty(transaction.Tags);
    }

    /// <summary>
    /// Given imported transactions and rules with case differences
    /// When Import is called
    /// Then rules should match case-insensitively
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithCaseDifference_MatchesCaseInsensitive()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var tag = _entities.CreateTag(1, "Groceries");
        var transaction = CreateTransaction("COSTCO WHOLESALE"); // uppercase
        var rule = CreateRule(1, "costco", workItem.InstrumentId, [tag]); // lowercase

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
    }

    /// <summary>
    /// Given imported transactions matching multiple rules with same tag
    /// When Import is called
    /// Then tags should be deduplicated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithDuplicateTagsFromRules_DeduplicatesTags()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var tag = _entities.CreateTag(1, "Groceries");
        var transaction = CreateTransaction("Costco Wholesale");

        // Both rules have the same tag
        var rule1 = CreateRule(1, "Costco", workItem.InstrumentId, [tag]);
        var rule2 = CreateRule(2, "Wholesale", workItem.InstrumentId, [tag]);

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule1, rule2]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Single(transaction.Tags);
    }

    /// <summary>
    /// Given imported transactions matching multiple rules
    /// When Import is called
    /// Then all matching tags should be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithMultipleMatchingRules_AppliesAllTags()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var groceriesTag = _entities.CreateTag(1, "Groceries");
        var wholesaleTag = _entities.CreateTag(2, "Wholesale");
        var transaction = CreateTransaction("Costco Wholesale");

        var rule1 = CreateRule(1, "Costco", workItem.InstrumentId, [groceriesTag]);
        var rule2 = CreateRule(2, "Wholesale", workItem.InstrumentId, [wholesaleTag]);

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule1, rule2]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Equal(2, transaction.Tags.Count());
        Assert.Contains(transaction.Tags, t => t.Id == groceriesTag.Id);
        Assert.Contains(transaction.Tags, t => t.Id == wholesaleTag.Id);
    }

    /// <summary>
    /// Given imported transactions with null description
    /// When Import is called
    /// Then no errors should occur
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithNullTransactionDescription_HandlesNullSafely()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var tag = _entities.CreateTag(1, "Groceries");
        var transaction = CreateTransaction(null);
        var rule = CreateRule(1, "Costco", workItem.InstrumentId, [tag]);

        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, [rule]);

        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert - Should complete without error and no tags applied
        Assert.Empty(transaction.Tags);
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Given a work item for a non-existent instrument
    /// When Import is called
    /// Then the error should be logged but not thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WhenInstrumentNotFound_DoesNotThrow()
    {
        // Arrange
        var workItem = CreateWorkItem();
        _instrumentRepositoryMock
            .Setup(r => r.Get(workItem.InstrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainInstrument?)null);

        var service = CreateService();

        // Act - Should complete without throwing
        await service.Import(workItem);

        // Assert - Save should not be called since we hit an error
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a work item for an account without importer support
    /// When Import is called
    /// Then the error should be logged but not thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WhenImporterNotSupported_DoesNotThrow()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        SetupInstrumentRepository(workItem.InstrumentId, instrument);

        _importerFactoryMock
            .Setup(f => f.Create(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IImporter?)null);

        var service = CreateService();

        // Act - Should complete without throwing
        await service.Import(workItem);

        // Assert - Save should not be called since we hit an error
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given an importer that throws during import
    /// When Import is called
    /// Then the error should be logged but not thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WhenImporterThrows_DoesNotThrow()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);

        _importerMock
            .Setup(i => i.Import(workItem.InstrumentId, workItem.AccountId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Import failed"));

        var service = CreateService();

        // Act - Should complete without throwing
        await service.Import(workItem);

        // Assert - Save should not be called since we hit an error
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given no imported transactions
    /// When Import is called
    /// Then save should still be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithNoTransactions_SavesChanges()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        SetupSuccessfulImport(workItem, instrument, []);
        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given no rules for the instrument
    /// When Import is called
    /// Then import should complete without applying tags
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Import_WithNoRules_CompletesWithoutTags()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var instrument = CreateInstrument(workItem.InstrumentId);
        var transaction = CreateTransaction("Costco Purchase");
        SetupSuccessfulImport(workItem, instrument, [transaction]);
        SetupRuleRepository(workItem.InstrumentId, []);
        var service = CreateService();

        // Act
        await service.Import(workItem);

        // Assert
        Assert.Empty(transaction.Tags);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helpers

    private IImportTransactionsService CreateService() =>
        new ImportTransactionsService(
            _instrumentRepositoryMock.Object,
            _ruleRepositoryMock.Object,
            _importerFactoryMock.Object,
            _unitOfWorkMock.Object,
            _logger);

    private void SetupSuccessfulImport(ImportWorkItem workItem, DomainInstrument instrument, IEnumerable<DomainTransaction> transactions)
    {
        SetupInstrumentRepository(workItem.InstrumentId, instrument);
        SetupImporterFactory(workItem, _importerMock.Object);
        SetupImporterImport(workItem, transactions);
    }

    private void SetupInstrumentRepository(Guid instrumentId, DomainInstrument? instrument)
    {
        _instrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);
    }

    private void SetupImporterFactory(ImportWorkItem workItem, IImporter importer)
    {
        _importerFactoryMock
            .Setup(f => f.Create(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importer);
    }

    private void SetupImporterImport(ImportWorkItem workItem, IEnumerable<DomainTransaction> transactions)
    {
        _importerMock
            .Setup(i => i.Import(workItem.InstrumentId, workItem.AccountId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransactionImportResult(transactions));
    }

    private void SetupRuleRepository(Guid instrumentId, IEnumerable<Rule> rules)
    {
        _ruleRepositoryMock
            .Setup(r => r.GetForInstrument(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);
    }

    private DomainInstrument CreateInstrument(Guid id)
    {
        return new LogicalAccount(id, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            AccountType = AccountType.Transaction,
        };
    }

    private static ImportWorkItem CreateWorkItem(byte[]? fileData = null)
    {
        var user = new User
        {
            Id = TestModels.UserId,
            EmailAddress = "test@example.com",
            Currency = "AUD",
            FamilyId = Guid.NewGuid(),
        };
        return new ImportWorkItem(
            TestModels.AccountId,
            Guid.NewGuid(),
            user,
            fileData ?? [1, 2, 3]);
    }

    private DomainTransaction CreateTransaction(string? description)
    {
        return DomainTransaction.Create(
            TestModels.AccountId,
            TestModels.UserId,
            -50m,
            description,
            DateTime.UtcNow,
            null,
            "Test",
            null);
    }

    private static Rule CreateRule(int id, string contains, Guid instrumentId, ICollection<DomainTag> tags)
    {
        return new Rule(id)
        {
            Contains = contains,
            InstrumentId = instrumentId,
            Tags = tags,
        };
    }

    #endregion
}
