#nullable enable
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTagPurposeHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class DeleteTagPurposeTests
{
    private readonly TestMocks _mocks;

    public DeleteTagPurposeTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// Given an account with an existing purpose assignment
    /// When the handler runs
    /// Then the assignment is removed.
    /// </summary>
    [Fact]
    public async Task Handle_RemovesAssignment()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);
        entity.SetTagPurpose(TagPurpose.Interest, 7);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new DeleteTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new DeleteTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(entity.TagPurposes);
    }

    /// <summary>
    /// Given an account with no existing assignment for the requested purpose
    /// When the handler runs
    /// Then it is a no-op (no exception, save still occurs).
    /// </summary>
    [Fact]
    public async Task Handle_NoExistingAssignment_IsNoOp()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new DeleteTagPurposeHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new DeleteTagPurpose { InstrumentId = accountId, Purpose = TagPurpose.Interest };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(entity.TagPurposes);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
