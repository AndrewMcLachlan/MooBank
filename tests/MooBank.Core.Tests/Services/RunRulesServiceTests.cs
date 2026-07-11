using Asm.Domain;
using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="RunRulesService"/> class.
/// Tests verify rule matching, tag application, and notes generation logic.
/// </summary>
public class RunRulesServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IInstrumentRepository> _instrumentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TestEntities _entities = new();

    public RunRulesServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _instrumentRepositoryMock = new Mock<IInstrumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #region Rule Matching

    /// <summary>
    /// Given a transaction with description containing rule's Contains value
    /// When RunRules is called
    /// Then the rule's tags should be applied to the transaction
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithMatchingRule_AppliesTagsToTransaction()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("Costco Purchase");
        var rule = CreateRule(1, "Costco", accountId, [tag]);

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a transaction with description NOT containing rule's Contains value
    /// When RunRules is called
    /// Then the rule's tags should NOT be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithNonMatchingRule_DoesNotApplyTags()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("Walmart Purchase");
        var rule = CreateRule(1, "Costco", accountId, [tag]);

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(transaction.Tags);
    }

    /// <summary>
    /// Given a transaction matching multiple rules
    /// When RunRules is called
    /// Then all matching rules' tags should be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithMultipleMatchingRules_AppliesAllTags()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var groceriesTag = _entities.CreateTag(1, "Groceries");
        var wholesaleTag = _entities.CreateTag(2, "Wholesale");

        var transaction = CreateTransaction("Costco Wholesale Purchase");
        var rule1 = CreateRule(1, "Costco", accountId, [groceriesTag]);
        var rule2 = CreateRule(2, "Wholesale", accountId, [wholesaleTag]);

        SetupRepositories(accountId, [transaction], [rule1, rule2]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, transaction.Tags.Count());
        Assert.Contains(transaction.Tags, t => t.Id == groceriesTag.Id);
        Assert.Contains(transaction.Tags, t => t.Id == wholesaleTag.Id);
    }

    /// <summary>
    /// Given a rule with multiple tags
    /// When RunRules is called and rule matches
    /// Then all tags from the rule should be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithRuleHavingMultipleTags_AppliesAllTagsFromRule()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Essentials");
        var tag3 = _entities.CreateTag(3, "Weekly");

        var transaction = CreateTransaction("Costco Shopping");
        var rule = CreateRule(1, "Costco", accountId, [tag1, tag2, tag3]);

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, transaction.Tags.Count());
    }

    /// <summary>
    /// Given a transaction description with different case than rule's Contains value
    /// When RunRules is called
    /// Then the rule should still match (case-insensitive)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithDifferentCase_MatchesCaseInsensitive()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("COSTCO purchase"); // uppercase
        var rule = CreateRule(1, "costco", accountId, [tag]); // lowercase

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
    }

    /// <summary>
    /// Given multiple transactions with some matching rules
    /// When RunRules is called
    /// Then only matching transactions should have tags applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithMultipleTransactions_AppliesTagsOnlyToMatching()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var matchingTransaction = CreateTransaction("Costco Purchase");
        var nonMatchingTransaction = CreateTransaction("Amazon Order");

        var rule = CreateRule(1, "Costco", accountId, [tag]);

        SetupRepositories(accountId, [matchingTransaction, nonMatchingTransaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(matchingTransaction.Tags, t => t.Id == tag.Id);
        Assert.Empty(nonMatchingTransaction.Tags);
    }

    #endregion

    #region Notes Generation

    /// <summary>
    /// Given a matching rule with description and transaction without notes
    /// When RunRules is called
    /// Then the rule's description should be set as transaction notes
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithRuleDescriptionAndNoTransactionNotes_SetsNotes()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("Costco Purchase");
        Assert.Null(transaction.Notes);

        var rule = CreateRule(1, "Costco", accountId, [tag], "Costco grocery shopping");

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Costco grocery shopping", transaction.Notes);
    }

    /// <summary>
    /// Given a matching rule with description and transaction WITH existing notes
    /// When RunRules is called
    /// Then the existing transaction notes should NOT be overwritten
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithExistingTransactionNotes_DoesNotOverwriteNotes()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("Costco Purchase");
        transaction.Notes = "My existing notes";

        var rule = CreateRule(1, "Costco", accountId, [tag], "Rule description");

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("My existing notes", transaction.Notes);
    }

    /// <summary>
    /// Given multiple matching rules with descriptions
    /// When RunRules is called
    /// Then all descriptions should be joined in notes
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithMultipleRuleDescriptions_JoinsDescriptions()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Wholesale");

        var transaction = CreateTransaction("Costco Wholesale");

        var rule1 = CreateRule(1, "Costco", accountId, [tag1], "Costco shopping");
        var rule2 = CreateRule(2, "Wholesale", accountId, [tag2], "Bulk purchase");

        SetupRepositories(accountId, [transaction], [rule1, rule2]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Costco shopping", transaction.Notes);
        Assert.Contains("Bulk purchase", transaction.Notes);
        Assert.Contains(". ", transaction.Notes); // Joined with period
    }

    /// <summary>
    /// Given matching rules where some have empty descriptions
    /// When RunRules is called
    /// Then only non-empty descriptions should be in notes
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithMixedRuleDescriptions_IgnoresEmptyDescriptions()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Wholesale");

        var transaction = CreateTransaction("Costco Wholesale");

        var ruleWithDescription = CreateRule(1, "Costco", accountId, [tag1], "Has description");
        var ruleWithoutDescription = CreateRule(2, "Wholesale", accountId, [tag2], null);

        SetupRepositories(accountId, [transaction], [ruleWithDescription, ruleWithoutDescription]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Has description", transaction.Notes);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given no transactions for the account
    /// When RunRules is called
    /// Then no errors should occur and save should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithNoTransactions_CompletesWithoutError()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");
        var rule = CreateRule(1, "Costco", accountId, [tag]);

        SetupRepositories(accountId, [], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert - nothing matched, so nothing needs saving
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given no rules for the account
    /// When RunRules is called
    /// Then no tags should be applied and save should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithNoRules_CompletesWithoutApplyingTags()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var transaction = CreateTransaction("Costco Purchase");

        SetupRepositories(accountId, [transaction], []);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert - nothing matched, so nothing needs saving
        Assert.Empty(transaction.Tags);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a transaction with null description
    /// When RunRules is called
    /// Then no errors should occur (null-safe matching)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithNullDescription_HandlesNullSafely()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var tag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction(null);
        var rule = CreateRule(1, "Costco", accountId, [tag]);

        SetupRepositories(accountId, [transaction], [rule]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert - Should not throw and should not match
        Assert.Empty(transaction.Tags);
    }

    /// <summary>
    /// Given duplicate tags from multiple matching rules
    /// When RunRules is called
    /// Then tags should be deduplicated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WithDuplicateTags_DeduplicatesTags()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        var sharedTag = _entities.CreateTag(1, "Groceries");

        var transaction = CreateTransaction("Costco Wholesale");

        // Both rules have the same tag
        var rule1 = CreateRule(1, "Costco", accountId, [sharedTag]);
        var rule2 = CreateRule(2, "Wholesale", accountId, [sharedTag]);

        SetupRepositories(accountId, [transaction], [rule1, rule2]);
        var service = CreateService();

        // Act
        await service.RunRules(accountId, TestContext.Current.CancellationToken);

        // Assert - Tag should only appear once
        Assert.Single(transaction.Tags);
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Given the repository throws an exception
    /// When RunRules is called
    /// Then the exception should propagate
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunRules_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var accountId = TestModels.AccountId;
        _transactionRepositoryMock
            .Setup(r => r.GetTransactionDescriptions(accountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunRules(accountId, TestContext.Current.CancellationToken));
    }

    #endregion

    #region Helpers

    private IRunRulesService CreateService() =>
        new RunRulesService(
            _transactionRepositoryMock.Object,
            _instrumentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<RunRulesService>.Instance);

    private void SetupRepositories(Guid accountId, IEnumerable<Transaction> transactions, IEnumerable<Rule> rules)
    {
        var transactionList = transactions.ToList();

        _transactionRepositoryMock
            .Setup(r => r.GetTransactionDescriptions(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionList.Select(t => new TransactionDescription(t.Id, t.Description)).ToList());

        _transactionRepositoryMock
            .Setup(r => r.GetTransactions(accountId, It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, IEnumerable<Guid> ids, CancellationToken _) => transactionList.Where(t => ids.Contains(t.Id)).ToList());

        var instrument = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Rules = [.. rules],
        };

        _instrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);
    }

    private Transaction CreateTransaction(string? description)
    {
        return Transaction.Create(
            TestModels.AccountId,
            TestModels.UserId,
            -50m,
            description,
            DateTime.UtcNow,
            null,
            "Test",
            null);
    }

    private static Rule CreateRule(int id, string contains, Guid instrumentId, ICollection<Tag> tags, string? description = null)
    {
        return new Rule(id)
        {
            Contains = contains,
            InstrumentId = instrumentId,
            Tags = tags,
            Description = description,
        };
    }

    #endregion
}
