#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Bills;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_OwnedAccounts_ReturnsBills()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 15));
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 2, 15));
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill1, bill2]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll { PageSize = 10, PageNumber = 1 };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Results.Count());
    }

    [Fact]
    public async Task Handle_NoBills_ReturnsEmptyResults()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll { PageSize = 10, PageNumber = 1 };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_StartDateFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var oldBill = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2023, 6, 1));
        var newBill = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 6, 1));
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [oldBill, newBill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll
        {
            PageSize = 10,
            PageNumber = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Equal(new DateOnly(2024, 6, 1), result.Results.First().IssueDate);
    }

    [Fact]
    public async Task Handle_EndDateFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var oldBill = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2023, 6, 1));
        var newBill = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 6, 1));
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [oldBill, newBill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll
        {
            PageSize = 10,
            PageNumber = 1,
            EndDate = new DateOnly(2023, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Equal(new DateOnly(2023, 6, 1), result.Results.First().IssueDate);
    }

    [Fact]
    public async Task Handle_AccountIdFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var targetAccountId = Guid.NewGuid();
        var bill1 = TestEntities.CreateBill(id: 1);
        var bill2 = TestEntities.CreateBill(id: 2);
        var account1 = TestEntities.CreateAccountWithOwner(id: targetAccountId, ownerId: userId, bills: [bill1]);
        var account2 = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(account1, account2);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll
        {
            PageSize = 10,
            PageNumber = 1,
            AccountId = targetAccountId
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_UtilityTypeFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var elecBill = TestEntities.CreateBill(id: 1);
        var gasBill = TestEntities.CreateBill(id: 2);
        var elecAccount = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [elecBill]);
        var gasAccount = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Gas,
            bills: [gasBill]);

        var queryable = TestEntities.CreateAccountQueryable(elecAccount, gasAccount);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll
        {
            PageSize = 10,
            PageNumber = 1,
            UtilityType = UtilityType.Electricity
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bills = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateBill(id: i, issueDate: new DateOnly(2024, 1, i)))
            .ToList();
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: bills);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll { PageSize = 10, PageNumber = 2 };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Results.Count());
    }

    [Fact]
    public async Task Handle_OrdersByIssueDateDescending()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 1));
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 3, 1));
        var bill3 = TestEntities.CreateBill(id: 3, issueDate: new DateOnly(2024, 2, 1));
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [bill1, bill2, bill3]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll { PageSize = 10, PageNumber = 1 };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var bills = result.Results.ToList();
        Assert.Equal(new DateOnly(2024, 3, 1), bills[0].IssueDate);
        Assert.Equal(new DateOnly(2024, 2, 1), bills[1].IssueDate);
        Assert.Equal(new DateOnly(2024, 1, 1), bills[2].IssueDate);
    }

    [Fact]
    public async Task Handle_OtherUserBills_ExcludesOtherUserBills()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var otherUserId = Guid.NewGuid();
        var myBill = TestEntities.CreateBill(id: 1);
        var otherBill = TestEntities.CreateBill(id: 2);
        var myAccount = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [myBill]);
        var otherAccount = TestEntities.CreateAccountWithOwner(ownerId: otherUserId, bills: [otherBill]);

        var queryable = TestEntities.CreateAccountQueryable(myAccount, otherAccount);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll { PageSize = 10, PageNumber = 1 };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
    }
}
