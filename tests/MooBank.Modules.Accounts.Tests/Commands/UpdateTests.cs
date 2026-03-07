using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsLogicalAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(
            id: accountId,
            name: "Old Name",
            accountType: AccountType.Transaction);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(
            id: accountId,
            name: "Updated Name",
            description: "Updated description",
            accountType: AccountType.Savings,
            controller: Controller.Manual,
            includeInBudget: true,
            shareWithFamily: true);
        var command = new Update(updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(AccountType.Savings, result.AccountType);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(
            id: accountId,
            name: "Original",
            accountType: AccountType.Transaction,
            controller: Controller.Manual,
            includeInBudget: false,
            shareWithFamily: false);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(
            id: accountId,
            name: "Modified",
            description: "New description",
            accountType: AccountType.Credit,
            controller: Controller.Import,
            includeInBudget: true,
            shareWithFamily: true);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Modified", existingEntity.Name);
        Assert.Equal("New description", existingEntity.Description);
        Assert.Equal(AccountType.Credit, existingEntity.AccountType);
        Assert.Equal(Controller.Import, existingEntity.Controller);
        Assert.True(existingEntity.IncludeInBudget);
        Assert.True(existingEntity.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.LogicalAccountRepositoryMock.Verify(r => r.Update(existingEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId, groupId: groupId);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(groupId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGroupId_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        _mocks.SecurityFailGroupPermission();

        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId, groupId: Guid.NewGuid());
        var command = new Update(updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_WithGroupId_NoPermission_DoesNotFetchFromRepository()
    {
        // Arrange
        _mocks.SecurityFailGroupPermission();

        var accountId = Guid.NewGuid();

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId, groupId: Guid.NewGuid());
        var command = new Update(updateModel);

        // Act
        try
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        }
        catch (NotAuthorisedException)
        {
            // Expected
        }

        // Assert
        _mocks.LogicalAccountRepositoryMock.Verify(
            r => r.Get(It.IsAny<Guid>(), It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutGroupId_DoesNotCheckGroupPermission()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateLogicalAccount(id: accountId);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId, groupId: null);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(id: accountId);
        var command = new Update(updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ChangeControllerToManual_ClearsImporterTypes()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            instrumentId: accountId,
            importerTypeId: 5);
        var existingEntity = TestEntities.CreateLogicalAccount(
            id: accountId,
            controller: Controller.Import,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = TestEntities.CreateLogicalAccountModel(
            id: accountId,
            name: "Now Manual",
            controller: Controller.Manual);
        var command = new Update(updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(Controller.Manual, existingEntity.Controller);
        Assert.Null(institutionAccount.ImporterTypeId);
    }
}
