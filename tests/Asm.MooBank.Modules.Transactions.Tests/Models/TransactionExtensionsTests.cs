#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Asm.MooBank.Modules.Transactions.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Transactions.Tests.Models;

[Trait("Category", "Unit")]
public class TransactionExtensionsTests
{
    #region ToSimpleModel Tests

    [Fact]
    public void ToSimpleModel_DebitTransaction_ReturnsNegativeAmount()
    {
        // Arrange - Debit transaction with positive amount stored
        var transaction = TestEntities.CreateTransaction(
            amount: 100m,
            transactionType: TransactionType.Debit);

        // Act
        var model = transaction.ToSimpleModel();

        // Assert - Should be negative for display
        Assert.True(model.Amount < 0);
        Assert.Equal(-100m, model.Amount);
    }

    [Fact]
    public void ToSimpleModel_DebitTransaction_WithNegativeStored_ReturnsNegativeAmount()
    {
        // Arrange - Debit transaction with negative amount already stored
        var transaction = TestEntities.CreateTransaction(
            amount: -100m,
            transactionType: TransactionType.Debit);

        // Act
        var model = transaction.ToSimpleModel();

        // Assert - Should still be negative (uses Math.Abs then negates)
        Assert.True(model.Amount < 0);
        Assert.Equal(-100m, model.Amount);
    }

    [Fact]
    public void ToSimpleModel_CreditTransaction_ReturnsPositiveAmount()
    {
        // Arrange - Credit transaction with positive amount
        var transaction = TestEntities.CreateTransaction(
            amount: 100m,
            transactionType: TransactionType.Credit);

        // Act
        var model = transaction.ToSimpleModel();

        // Assert - Should return amount unchanged for credit
        Assert.Equal(100m, model.Amount);
    }

    [Fact]
    public void ToSimpleModel_CreditTransaction_PreservesOriginalAmount()
    {
        // Arrange - Credit transaction with specific amount
        var transaction = TestEntities.CreateTransaction(
            amount: 250.75m,
            transactionType: TransactionType.Credit);

        // Act
        var model = transaction.ToSimpleModel();

        // Assert - Should return exact amount unchanged
        Assert.Equal(250.75m, model.Amount);
    }

    [Fact]
    public void ToSimpleModel_MapsAllProperties()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactionTime = DateTime.UtcNow.AddDays(-1);

        var transaction = TestEntities.CreateTransaction(
            id: transactionId,
            accountId: accountId,
            amount: 100m,
            description: "Test Description",
            transactionTime: transactionTime,
            transactionType: TransactionType.Credit,
            reference: "REF123",
            notes: "Test notes",
            excludeFromReporting: true);

        // Act
        var model = transaction.ToSimpleModel();

        // Assert
        Assert.Equal(transactionId, model.Id);
        Assert.Equal(accountId, model.AccountId);
        Assert.Equal("Test Description", model.Description);
        Assert.Equal(transactionTime, model.TransactionTime);
        Assert.Equal(TransactionType.Credit, model.TransactionType);
        Assert.Equal("REF123", model.Reference);
        Assert.Equal("Test notes", model.Notes);
        Assert.True(model.ExcludeFromReporting);
    }

    #endregion

    #region ToModel Tests

    [Fact]
    public void ToModel_WithNullUser_ReturnsNullAccountHolderName()
    {
        // Arrange - Transaction without a User
        var transaction = TestEntities.CreateTransaction(
            accountHolderId: null);
        // User property is not set, so it should be null

        // Act
        var model = transaction.ToModel();

        // Assert
        Assert.Null(model.AccountHolderName);
    }

    [Fact]
    public void ToModel_MapsAllBasicProperties()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactionTime = DateTime.UtcNow.AddDays(-1);

        var transaction = TestEntities.CreateTransaction(
            id: transactionId,
            accountId: accountId,
            amount: -75.50m,
            description: "Test Purchase",
            transactionTime: transactionTime,
            transactionType: TransactionType.Debit,
            reference: "REF456",
            notes: "Some notes",
            excludeFromReporting: false);

        // Act
        var model = transaction.ToModel();

        // Assert
        Assert.Equal(transactionId, model.Id);
        Assert.Equal(accountId, model.AccountId);
        Assert.Equal(-75.50m, model.Amount);
        Assert.Equal("Test Purchase", model.Description);
        Assert.Equal(transactionTime, model.TransactionTime);
        Assert.Equal(TransactionType.Debit, model.TransactionType);
        Assert.Equal("REF456", model.Reference);
        Assert.Equal("Some notes", model.Notes);
        Assert.False(model.ExcludeFromReporting);
    }

    [Fact]
    public void ToModel_WithTags_MapsTags()
    {
        // Arrange
        var tag1 = TestEntities.CreateTag(1, "Groceries");
        var tag2 = TestEntities.CreateTag(2, "Food");

        var transaction = TestEntities.CreateTransaction(
            amount: -50m,
            tags: [tag1, tag2]);

        // Act
        var model = transaction.ToModel();

        // Assert
        Assert.NotNull(model.Tags);
        Assert.Equal(2, model.Tags.Count());
    }

    [Fact]
    public void ToModel_WithDeletedTags_ExcludesDeletedTags()
    {
        // Arrange
        var activeTag = TestEntities.CreateTag(1, "Active");
        var deletedTag = new Asm.MooBank.Domain.Entities.Tag.Tag(2)
        {
            Name = "Deleted",
            FamilyId = Guid.NewGuid(),
            Deleted = true,
        };

        var transaction = TestEntities.CreateTransaction(
            amount: -50m,
            tags: [activeTag, deletedTag]);

        // Act
        var model = transaction.ToModel();

        // Assert
        Assert.Single(model.Tags);
        Assert.Equal("Active", model.Tags.First().Name);
    }

    [Fact]
    public void ToModel_WithSplits_MapsSplits()
    {
        // Arrange
        var split1 = TestEntities.CreateTransactionSplit(amount: 30m);
        var split2 = TestEntities.CreateTransactionSplit(amount: 20m);

        var transaction = TestEntities.CreateTransaction(
            amount: -50m,
            splits: [split1, split2]);

        // Act
        var model = transaction.ToModel();

        // Assert
        Assert.NotNull(model.Splits);
        Assert.Equal(2, model.Splits.Count());
    }

    #endregion

    #region ToModel Collection Tests

    [Fact]
    public void ToModel_IEnumerable_MapsAllTransactions()
    {
        // Arrange
        var transactions = new[]
        {
            TestEntities.CreateTransaction(amount: -100m),
            TestEntities.CreateTransaction(amount: 200m, transactionType: TransactionType.Credit),
            TestEntities.CreateTransaction(amount: -50m),
        };

        // Act
        var models = transactions.AsEnumerable().ToModel().ToList();

        // Assert
        Assert.Equal(3, models.Count);
    }

    #endregion
}
