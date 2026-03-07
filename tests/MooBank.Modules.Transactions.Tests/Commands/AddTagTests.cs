#nullable enable
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class AddTagTests
{
    private readonly TestMocks _mocks;

    public AddTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsTagAndReturnsTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId);
        var newTag = TestEntities.CreateTag(id: tagId, name: "New Tag");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTag);

        var handler = new AddTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, transactionId, tagId);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId);
        var newTag = TestEntities.CreateTag(id: tagId, name: "New Tag");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTag);

        var handler = new AddTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, transactionId, tagId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TagAlreadyExists_ThrowsExistsException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Existing Tag");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new AddTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, transactionId, tagId);

        // Act & Assert
        await Assert.ThrowsAsync<ExistsException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TagAlreadyExists_DoesNotSave()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Existing Tag");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new AddTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, transactionId, tagId);

        // Act
        try
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        }
        catch (ExistsException)
        {
            // Expected
        }

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_FetchesTagFromRepository()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 10;

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId);
        var newTag = TestEntities.CreateTag(id: tagId, name: "Fetched Tag");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTag);

        var handler = new AddTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, transactionId, tagId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.TagRepositoryMock.Verify(r => r.Get(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
