#nullable enable
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

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

        SetupTag(42);

        var handler = CreateHandler();

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

        SetupTag(99);

        var handler = CreateHandler();

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 99 };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var assignment = Assert.Single(entity.TagPurposes);
        Assert.Equal(99, assignment.TagId);
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

        SetupTag(1);

        var handler = CreateHandler();

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 1 };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a tag that does not belong to the user's family
    /// When the handler runs
    /// Then a NotFoundException is thrown and no changes are saved.
    /// </summary>
    [Fact]
    public async Task Handle_TagNotInFamily_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // The family-scoped tag repository throws when the tag is not visible to the user's family.
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(42, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Transaction tag with id 42 was not found"));

        var handler = CreateHandler();

        var command = new SetTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest, TagId = 42 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Empty(entity.TagPurposes);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private SetTagPurposeHandler CreateHandler() =>
        new(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

    private void SetupTag(int tagId) =>
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DomainTag(tagId) { Name = "Test Tag", FamilyId = Guid.NewGuid() });
}
