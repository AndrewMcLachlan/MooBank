#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Models.InstitutionAccount;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands.InstitutionAccounts;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsInstitutionAccount()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            name: "Old Name",
            institutionId: 1);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            controller: Controller.Manual,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Updated Name",
            InstitutionId = 2,
            ImporterTypeId = null,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(2, result.InstitutionId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            name: "Original",
            institutionId: 1);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            controller: Controller.Manual,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Modified",
            InstitutionId = 3,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Modified", institutionAccount.Name);
        Assert.Equal(3, institutionAccount.InstitutionId);
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
            controller: Controller.Manual,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Updated",
            InstitutionId = 1,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Updated",
            InstitutionId = 1,
        };
        var command = new Update(logicalAccountId, Guid.NewGuid(), updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ImportControllerWithImporterType_UpdatesImporterType()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            importerTypeId: 1);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            controller: Controller.Import,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.GetImporterType(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImporterType { ImporterTypeId = 5, Name = "Test Importer", Type = "TestType" });

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Import Account",
            InstitutionId = 1,
            ImporterTypeId = 5,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.ImporterTypeId);
        Assert.Equal(5, institutionAccount.ImporterTypeId);
    }

    [Fact]
    public async Task Handle_ImportControllerWithoutImporterType_ThrowsInvalidOperationException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            controller: Controller.Import,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Import Account",
            InstitutionId = 1,
            ImporterTypeId = null,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ImportControllerWithUnknownImporterType_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            controller: Controller.Import,
            institutionAccounts: [institutionAccount]);

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Get(logicalAccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logicalAccount);
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.GetImporterType(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImporterType?)null);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.LogicalAccountRepositoryMock.Object);

        var updateModel = new UpdateInstitutionAccount
        {
            Name = "Import Account",
            InstitutionId = 1,
            ImporterTypeId = 999,
        };
        var command = new Update(logicalAccountId, institutionAccountId, updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }
}
