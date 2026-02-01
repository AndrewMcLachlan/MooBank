using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands.InstitutionAccounts;

[Trait("Category", "Unit")]
public class CloseTests
{
    private readonly TestMocks _mocks;

    public CloseTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ClosesAndReturnsInstitutionAccount()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            name: "Open Account",
            closedDate: null);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, institutionAccountId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClosedDate);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result.ClosedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsClosedDateOnEntity()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            closedDate: null);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, institutionAccountId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(institutionAccount.ClosedDate);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), institutionAccount.ClosedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, institutionAccountId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentLogicalAccount_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentInstitutionAccount_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(instrumentId: logicalAccountId);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleInstitutionAccounts_ClosesCorrectOne()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var targetAccount = TestEntities.CreateInstitutionAccount(
            id: targetId,
            instrumentId: logicalAccountId,
            closedDate: null);
        var otherAccount = TestEntities.CreateInstitutionAccount(
            id: otherId,
            instrumentId: logicalAccountId,
            closedDate: null);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [targetAccount, otherAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CloseHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Close(logicalAccountId, targetId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(targetAccount.ClosedDate);
        Assert.Null(otherAccount.ClosedDate);
    }
}
