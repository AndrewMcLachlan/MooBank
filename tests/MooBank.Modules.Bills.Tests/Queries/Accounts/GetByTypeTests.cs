#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Accounts;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Accounts;

[Trait("Category", "Unit")]
public class GetByTypeTests
{
    private readonly TestMocks _mocks;

    public GetByTypeTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_FilterByElectricity_ReturnsOnlyElectricityAccounts()
    {
        // Arrange
        var elecId = Guid.NewGuid();
        var gasId = Guid.NewGuid();

        var accounts = new[]
        {
            TestEntities.CreateAccount(id: elecId, name: "Electricity", utilityType: UtilityType.Electricity),
            TestEntities.CreateAccount(id: gasId, name: "Gas", utilityType: UtilityType.Gas),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [elecId, gasId]);
        _mocks.SetUser(user);

        var handler = new GetByTypeHandler(queryable, _mocks.User);
        var query = new GetByType(UtilityType.Electricity);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Electricity", resultList[0].Name);
        Assert.Equal(UtilityType.Electricity, resultList[0].UtilityType);
    }

    [Fact]
    public async Task Handle_MultipleAccountsOfSameType_ReturnsAll()
    {
        // Arrange
        var elec1 = Guid.NewGuid();
        var elec2 = Guid.NewGuid();

        var accounts = new[]
        {
            TestEntities.CreateAccount(id: elec1, name: "Elec Provider 1", utilityType: UtilityType.Electricity),
            TestEntities.CreateAccount(id: elec2, name: "Elec Provider 2", utilityType: UtilityType.Electricity),
            TestEntities.CreateAccount(name: "Gas", utilityType: UtilityType.Gas),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [elec1, elec2]);
        _mocks.SetUser(user);

        var handler = new GetByTypeHandler(queryable, _mocks.User);
        var query = new GetByType(UtilityType.Electricity);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_NoMatchingType_ReturnsEmptyList()
    {
        // Arrange
        var gasId = Guid.NewGuid();
        var accounts = new[]
        {
            TestEntities.CreateAccount(id: gasId, name: "Gas", utilityType: UtilityType.Gas),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [gasId]);
        _mocks.SetUser(user);

        var handler = new GetByTypeHandler(queryable, _mocks.User);
        var query = new GetByType(UtilityType.Water);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FiltersToAccessibleAccountsOnly()
    {
        // Arrange
        var accessibleId = Guid.NewGuid();
        var inaccessibleId = Guid.NewGuid();

        var accounts = new[]
        {
            TestEntities.CreateAccount(id: accessibleId, name: "Accessible Elec", utilityType: UtilityType.Electricity),
            TestEntities.CreateAccount(id: inaccessibleId, name: "Inaccessible Elec", utilityType: UtilityType.Electricity),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [accessibleId]);
        _mocks.SetUser(user);

        var handler = new GetByTypeHandler(queryable, _mocks.User);
        var query = new GetByType(UtilityType.Electricity);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Accessible Elec", resultList[0].Name);
    }

    [Theory]
    [InlineData(UtilityType.Electricity)]
    [InlineData(UtilityType.Gas)]
    [InlineData(UtilityType.Water)]
    [InlineData(UtilityType.Phone)]
    [InlineData(UtilityType.Internet)]
    [InlineData(UtilityType.Other)]
    public async Task Handle_DifferentUtilityTypes_FiltersCorrectly(UtilityType type)
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var accounts = new[]
        {
            TestEntities.CreateAccount(id: targetId, name: "Target", utilityType: type),
            TestEntities.CreateAccount(name: "Other", utilityType: UtilityType.Other),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [targetId]);
        _mocks.SetUser(user);

        var handler = new GetByTypeHandler(queryable, _mocks.User);
        var query = new GetByType(type);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(type, resultList[0].UtilityType);
    }
}
