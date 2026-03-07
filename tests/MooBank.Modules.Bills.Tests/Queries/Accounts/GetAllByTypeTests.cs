#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Accounts;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Accounts;

[Trait("Category", "Unit")]
public class GetAllByTypeTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_AccessibleAccounts_ReturnsGroupedByType()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var bill1 = TestEntities.CreateBill(issueDate: new DateOnly(2024, 1, 15));
        var bill2 = TestEntities.CreateBill(issueDate: new DateOnly(2024, 2, 15));

        var elecAccount = TestEntities.CreateAccount(
            id: accountId1,
            name: "AGL Electricity",
            utilityType: UtilityType.Electricity,
            bills: [bill1]);
        var gasAccount = TestEntities.CreateAccount(
            id: accountId2,
            name: "Origin Gas",
            utilityType: UtilityType.Gas,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(elecAccount, gasAccount);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId1, accountId2]));

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.UtilityType == UtilityType.Electricity);
        Assert.Contains(result, r => r.UtilityType == UtilityType.Gas);
    }

    [Fact]
    public async Task Handle_MultipleAccountsSameType_GroupsTogether()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var bill1 = TestEntities.CreateBill(issueDate: new DateOnly(2024, 1, 15));
        var bill2 = TestEntities.CreateBill(issueDate: new DateOnly(2024, 2, 15));

        var elecAccount1 = TestEntities.CreateAccount(
            id: accountId1,
            name: "AGL Electricity",
            utilityType: UtilityType.Electricity,
            bills: [bill1]);
        var elecAccount2 = TestEntities.CreateAccount(
            id: accountId2,
            name: "Origin Electricity",
            utilityType: UtilityType.Electricity,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(elecAccount1, elecAccount2);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId1, accountId2]));

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        var summary = result.First();
        Assert.Equal(UtilityType.Electricity, summary.UtilityType);
        Assert.Equal(2, summary.Accounts.Count());
    }

    [Fact]
    public async Task Handle_NoAccessibleAccounts_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill();
        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [])); // No access

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_SharedAccounts_IncludesSharedAccounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill(issueDate: new DateOnly(2024, 3, 1));
        var account = TestEntities.CreateAccount(
            id: accountId,
            name: "Shared Water",
            utilityType: UtilityType.Water,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [], sharedAccounts: [accountId]));

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal(UtilityType.Water, result.First().UtilityType);
    }

    [Fact]
    public async Task Handle_ReturnsEarliestBillDate()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var oldBill = TestEntities.CreateBill(issueDate: new DateOnly(2023, 6, 1));
        var newBill = TestEntities.CreateBill(issueDate: new DateOnly(2024, 6, 1));
        var account = TestEntities.CreateAccount(
            id: accountId,
            utilityType: UtilityType.Internet,
            bills: [oldBill, newBill]);

        var queryable = TestEntities.CreateAccountQueryable(account);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateOnly(2023, 6, 1), result.First().From);
    }

    [Fact]
    public async Task Handle_ReturnsAccountNames()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill();
        var account = TestEntities.CreateAccount(
            id: accountId,
            name: "My Internet Provider",
            utilityType: UtilityType.Internet,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetAllByTypeHandler(queryable, _mocks.User);
        var query = new GetAllByType();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Contains("My Internet Provider", result.First().Accounts);
    }
}
