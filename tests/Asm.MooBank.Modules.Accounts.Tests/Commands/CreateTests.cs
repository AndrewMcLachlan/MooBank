#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsLogicalAccount()
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, _, _) => capturedEntity = e)
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "New Savings Account",
            Description = "Test description",
            Currency = "AUD",
            Balance = 5000m,
            InstitutionId = 1,
            AccountType = AccountType.Savings,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Savings Account", result.Name);
        Assert.Equal("Test description", result.Description);
        Assert.Equal(AccountType.Savings, result.AccountType);
        Assert.Equal(Controller.Manual, result.Controller);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        decimal capturedBalance = 0;
        DateOnly capturedDate = default;

        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, b, d) =>
            {
                capturedEntity = e;
                capturedBalance = b;
                capturedDate = d;
            })
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var openedDate = new DateOnly(2024, 1, 15);
        var command = new Create
        {
            Name = "New Account",
            Currency = "AUD",
            Balance = 2500m,
            InstitutionId = 2,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
            OpenedDate = openedDate,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.LogicalAccountRepositoryMock.Verify(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("New Account", capturedEntity.Name);
        Assert.Equal(2500m, capturedBalance);
        Assert.Equal(openedDate, capturedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Test",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesInstitutionAccount()
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, _, _) => capturedEntity = e)
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Account with Institution",
            Currency = "AUD",
            Balance = 1000m,
            InstitutionId = 5,
            ImporterTypeId = 3,
            AccountType = AccountType.Transaction,
            Controller = Controller.Import,
            IncludeInBudget = true,
            ShareWithFamily = false,
            OpenedDate = new DateOnly(2024, 6, 1),
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Single(capturedEntity.InstitutionAccounts);
        var institutionAccount = capturedEntity.InstitutionAccounts.First();
        Assert.Equal("Account with Institution", institutionAccount.Name);
        Assert.Equal(5, institutionAccount.InstitutionId);
        Assert.Equal(3, institutionAccount.ImporterTypeId);
        Assert.Equal(new DateOnly(2024, 6, 1), institutionAccount.OpenedDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAccountHolder()
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, _, _) => capturedEntity = e)
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Test",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Single(capturedEntity.Owners);
        Assert.Equal(_mocks.User.Id, capturedEntity.Owners.First().UserId);
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Group Account",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
            GroupId = groupId,
        };

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

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Group Account",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
            GroupId = Guid.NewGuid(),
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_WithoutGroupId_DoesNotCheckGroupPermission()
    {
        // Arrange
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Personal Account",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
            GroupId = null,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(It.IsAny<Guid>()), Times.Never);
    }

    [Theory]
    [InlineData(AccountType.Transaction)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.Credit)]
    [InlineData(AccountType.Superannuation)]
    [InlineData(AccountType.Loan)]
    [InlineData(AccountType.Investment)]
    public async Task Handle_DifferentAccountTypes_SetsCorrectType(AccountType expectedType)
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, _, _) => capturedEntity = e)
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Test Account",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = expectedType,
            Controller = Controller.Manual,
            IncludeInBudget = true,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedType, result.AccountType);
        Assert.Equal(expectedType, capturedEntity!.AccountType);
    }

    [Theory]
    [InlineData(Controller.Manual)]
    [InlineData(Controller.Import)]
    public async Task Handle_DifferentControllers_SetsCorrectController(Controller expectedController)
    {
        // Arrange
        LogicalAccount? capturedEntity = null;
        _mocks.LogicalAccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateOnly>()))
            .Callback<LogicalAccount, decimal, DateOnly>((e, _, _) => capturedEntity = e)
            .Returns<LogicalAccount, decimal, DateOnly>((e, _, _) => e);

        var handler = new CreateHandler(
            _mocks.LogicalAccountRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create
        {
            Name = "Test Account",
            Currency = "AUD",
            Balance = 100m,
            InstitutionId = 1,
            AccountType = AccountType.Transaction,
            Controller = expectedController,
            IncludeInBudget = true,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedController, result.Controller);
        Assert.Equal(expectedController, capturedEntity!.Controller);
    }
}
