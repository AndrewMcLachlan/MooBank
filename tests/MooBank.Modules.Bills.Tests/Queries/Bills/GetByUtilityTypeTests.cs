#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Bills;

[Trait("Category", "Unit")]
public class GetByUtilityTypeTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_MatchingUtilityType_ReturnsBills()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill = TestEntities.CreateBill(id: 1);
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Electricity,
            PageSize = 10,
            PageNumber = 1
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Single(result.Results);
    }

    [Fact]
    public async Task Handle_NonMatchingUtilityType_ReturnsEmpty()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill = TestEntities.CreateBill(id: 1);
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Gas,
            PageSize = 10,
            PageNumber = 1
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_MultipleAccountsSameType_ReturnsBillsFromAll()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill1 = TestEntities.CreateBill(id: 1);
        var bill2 = TestEntities.CreateBill(id: 2);
        var account1 = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Gas,
            bills: [bill1]);
        var account2 = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Gas,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(account1, account2);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Gas,
            PageSize = 10,
            PageNumber = 1
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task Handle_StartDateFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var oldBill = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2023, 6, 1));
        var newBill = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 6, 1));
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Water,
            bills: [oldBill, newBill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Water,
            PageSize = 10,
            PageNumber = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_EndDateFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var oldBill = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2023, 6, 1));
        var newBill = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 6, 1));
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Internet,
            bills: [oldBill, newBill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Internet,
            PageSize = 10,
            PageNumber = 1,
            EndDate = new DateOnly(2023, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_AccountIdFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var targetAccountId = Guid.NewGuid();
        var bill1 = TestEntities.CreateBill(id: 1);
        var bill2 = TestEntities.CreateBill(id: 2);
        var account1 = TestEntities.CreateAccountWithOwner(
            id: targetAccountId,
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [bill1]);
        var account2 = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(account1, account2);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Electricity,
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
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bills = Enumerable.Range(1, 15)
            .Select(i => TestEntities.CreateBill(id: i, issueDate: new DateOnly(2024, 1, i)))
            .ToList();
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: bills);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Electricity,
            PageSize = 10,
            PageNumber = 2
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(15, result.Total);
        Assert.Equal(5, result.Results.Count());
    }

    [Fact]
    public async Task Handle_OrdersByIssueDateDescending()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 1));
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 3, 1));
        var bill3 = TestEntities.CreateBill(id: 3, issueDate: new DateOnly(2024, 2, 1));
        var account = TestEntities.CreateAccountWithOwner(
            ownerId: userId,
            utilityType: UtilityType.Gas,
            bills: [bill1, bill2, bill3]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetByUtilityTypeHandler(queryable, _mocks.User);
        var query = new GetByUtilityType
        {
            UtilityType = UtilityType.Gas,
            PageSize = 10,
            PageNumber = 1
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var bills = result.Results.ToList();
        Assert.Equal(new DateOnly(2024, 3, 1), bills[0].IssueDate);
        Assert.Equal(new DateOnly(2024, 2, 1), bills[1].IssueDate);
        Assert.Equal(new DateOnly(2024, 1, 1), bills[2].IssueDate);
    }
}
