#nullable enable
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="SetTagPurposeHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class SetTagPurposeTests
{
    private readonly TestMocks _mocks;

    public SetTagPurposeTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// Given an account with no existing tag for the requested purpose
    /// When a non-null TagId is supplied
    /// Then a new assignment is added to the account.
    /// </summary>
    [Fact]
    public async Task Handle_AddsNewTagPurpose()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new SetTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 42 };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var assignment = Assert.Single(entity.TagPurposes);
        Assert.Equal(TagPurpose.Interest, assignment.Purpose);
        Assert.Equal(42, assignment.TagId);
    }

    /// <summary>
    /// Given an account that already has the requested purpose set
    /// When a different TagId is supplied
    /// Then the existing assignment is updated in place (no duplicates).
    /// </summary>
    [Fact]
    public async Task Handle_ReplacesExistingTagPurpose()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);
        entity.SetTagPurpose(TagPurpose.Interest, 7);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new SetTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 99 };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var assignment = Assert.Single(entity.TagPurposes);
        Assert.Equal(99, assignment.TagId);
    }

    /// <summary>
    /// Given an account with an existing purpose assignment
    /// When TagId is null
    /// Then the assignment is removed.
    /// </summary>
    [Fact]
    public async Task Handle_NullTagId_RemovesAssignment()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);
        entity.SetTagPurpose(TagPurpose.Interest, 7);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new SetTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = null };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(entity.TagPurposes);
    }

    /// <summary>
    /// Given a valid command
    /// When the handler runs
    /// Then SaveChanges is invoked exactly once.
    /// </summary>
    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new SetTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 1 };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
