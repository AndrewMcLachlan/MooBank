#nullable enable
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Tests.Support;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class RemoveTagTests
{
    private readonly TestMocks _mocks;

    public RemoveTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesTagAndReturnsTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Tag to Remove");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new RemoveTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, transactionId, tagId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

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

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Tag to Remove");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new RemoveTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, transactionId, tagId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TagNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;
        var nonExistentTagId = 999;

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Existing Tag");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new RemoveTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, transactionId, nonExistentTagId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_TagNotFound_DoesNotSave()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;
        var nonExistentTagId = 999;

        var existingTag = TestEntities.CreateTag(id: tagId, name: "Existing Tag");
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, tags: [existingTag]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new RemoveTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, transactionId, nonExistentTagId);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoTagsOnTransaction_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var tagId = 5;

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new RemoveTagHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, transactionId, tagId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }
}
