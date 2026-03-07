#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Accounts;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Accounts;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_UserWithAccounts_ReturnsAccessibleAccounts()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var accountId3 = Guid.NewGuid();

        var accounts = new[]
        {
            TestEntities.CreateAccount(id: accountId1, name: "Account 1"),
            TestEntities.CreateAccount(id: accountId2, name: "Account 2"),
            TestEntities.CreateAccount(id: accountId3, name: "Account 3"),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [accountId1, accountId2]);
        _mocks.SetUser(user);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, a => a.Id == accountId1);
        Assert.Contains(resultList, a => a.Id == accountId2);
        Assert.DoesNotContain(resultList, a => a.Id == accountId3);
    }

    [Fact]
    public async Task Handle_UserWithSharedAccounts_ReturnsSharedAccounts()
    {
        // Arrange
        var ownedId = Guid.NewGuid();
        var sharedId = Guid.NewGuid();

        var accounts = new[]
        {
            TestEntities.CreateAccount(id: ownedId, name: "Owned"),
            TestEntities.CreateAccount(id: sharedId, name: "Shared"),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [ownedId], sharedAccounts: [sharedId]);
        _mocks.SetUser(user);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, a => a.Name == "Owned");
        Assert.Contains(resultList, a => a.Name == "Shared");
    }

    [Fact]
    public async Task Handle_UserWithNoAccounts_ReturnsEmptyList()
    {
        // Arrange
        var accounts = TestEntities.CreateSampleAccounts();
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var user = TestMocks.CreateTestUser(accounts: [], sharedAccounts: []);
        _mocks.SetUser(user);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateAccountQueryable([]);
        var user = TestMocks.CreateTestUser(accounts: [Guid.NewGuid()]);
        _mocks.SetUser(user);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_AccountsWithBills_ReturnsBillDates()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = new[]
        {
            TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 3, 1)),
            TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 6, 1)),
        };
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var user = TestMocks.CreateTestUser(accounts: [accountId]);
        _mocks.SetUser(user);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultAccount = result.Single();
        Assert.Equal(new DateOnly(2024, 3, 1), resultAccount.FirstBill);
        Assert.Equal(new DateOnly(2024, 6, 1), resultAccount.LatestBill);
    }
}
