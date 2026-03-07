#nullable enable
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;

namespace Asm.MooBank.Modules.Budgets.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsBudget()
    {
        // Arrange
        DomainBudget? capturedEntity = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Callback<DomainBudget>(e => capturedEntity = e)
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(2024);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Year);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainBudget? capturedEntity = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Callback<DomainBudget>(e => capturedEntity = e)
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(2024);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.BudgetRepositoryMock.Verify(r => r.Add(It.IsAny<DomainBudget>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal(2024, capturedEntity.Year);
        Assert.Equal(_mocks.User.FamilyId, capturedEntity.FamilyId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(2024);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsFamilyIdFromUser()
    {
        // Arrange
        var specificFamilyId = Guid.NewGuid();
        var user = TestMocks.CreateTestUser(familyId: specificFamilyId);
        _mocks.SetUser(user);

        DomainBudget? capturedEntity = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Callback<DomainBudget>(e => capturedEntity = e)
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(2024);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(specificFamilyId, capturedEntity.FamilyId);
    }

    [Theory]
    [InlineData((short)2020)]
    [InlineData((short)2024)]
    [InlineData((short)2030)]
    public async Task Handle_DifferentYears_SetsCorrectYear(short year)
    {
        // Arrange
        DomainBudget? capturedEntity = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Callback<DomainBudget>(e => capturedEntity = e)
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(year);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(year, result.Year);
        Assert.Equal(year, capturedEntity!.Year);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsEmptyLines()
    {
        // Arrange
        _mocks.BudgetRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainBudget>()))
            .Returns<DomainBudget>(e => e);

        var handler = new CreateHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);
        var command = new Create(2024);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.IncomeLines);
        Assert.Empty(result.ExpensesLines);
    }
}
