using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Models.InstitutionAccount;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands.InstitutionAccounts;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsInstitutionAccount()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var logicalAccount = TestEntities.CreateLogicalAccount(id: logicalAccountId);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var createModel = new CreateInstitutionAccount
        {
            Name = "New Institution Account",
            InstitutionId = 1,
            ImporterTypeId = null,
            OpenedDate = new DateOnly(2024, 1, 1),
        };
        var command = new Create(logicalAccountId, createModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Institution Account", result.Name);
        Assert.Equal(1, result.InstitutionId);
        Assert.Equal(new DateOnly(2024, 1, 1), result.OpenedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToLogicalAccount()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var logicalAccount = TestEntities.CreateLogicalAccount(id: logicalAccountId);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var createModel = new CreateInstitutionAccount
        {
            Name = "New Institution Account",
            InstitutionId = 2,
            ImporterTypeId = 1,
            OpenedDate = new DateOnly(2024, 6, 15),
        };
        var command = new Create(logicalAccountId, createModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(logicalAccount.InstitutionAccounts);
        var added = logicalAccount.InstitutionAccounts.First();
        Assert.Equal("New Institution Account", added.Name);
        Assert.Equal(2, added.InstitutionId);
        Assert.Equal(1, added.ImporterTypeId);
        Assert.Equal(new DateOnly(2024, 6, 15), added.OpenedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var logicalAccount = TestEntities.CreateLogicalAccount(id: logicalAccountId);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var createModel = new CreateInstitutionAccount
        {
            Name = "New Account",
            InstitutionId = 1,
            OpenedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        var command = new Create(logicalAccountId, createModel);

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

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var createModel = new CreateInstitutionAccount
        {
            Name = "New Account",
            InstitutionId = 1,
            OpenedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        var command = new Create(logicalAccountId, createModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WithImporterType_SetsImporterTypeId()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var logicalAccount = TestEntities.CreateLogicalAccount(id: logicalAccountId);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var createModel = new CreateInstitutionAccount
        {
            Name = "Import Account",
            InstitutionId = 1,
            ImporterTypeId = 5,
            OpenedDate = new DateOnly(2024, 1, 1),
        };
        var command = new Create(logicalAccountId, createModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.ImporterTypeId);
        Assert.Equal(5, logicalAccount.InstitutionAccounts.First().ImporterTypeId);
    }
}
