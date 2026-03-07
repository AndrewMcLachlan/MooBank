#nullable enable
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Bills;

[Trait("Category", "Unit")]
public class GetForAccountTests
{
    [Fact]
    public async Task Handle_AccountWithBills_ReturnsPagedResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = Enumerable.Range(1, 5)
            .Select(i => TestEntities.CreateBill(id: i, invoiceNumber: $"INV{i:D3}"))
            .ToArray();
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(5, result.Total);
        Assert.Equal(5, result.Results.Count());
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateBill(id: i))
            .ToArray();
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId, PageSize: 10, PageNumber: 1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Results.Count());
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsNextItems()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateBill(id: i))
            .ToArray();
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId, PageSize: 10, PageNumber: 2);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Results.Count());
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateBill(id: i))
            .ToArray();
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId, PageSize: 10, PageNumber: 3);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(5, result.Results.Count());
    }

    [Fact]
    public async Task Handle_AccountWithNoBills_ReturnsEmptyResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var account = TestEntities.CreateAccount();
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_DefaultPageSize_Returns20Items()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = Enumerable.Range(1, 30)
            .Select(i => TestEntities.CreateBill(id: i))
            .ToArray();
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetForAccountHandler(queryable);
        var query = new GetForAccount(accountId); // Default: PageSize=20, PageNumber=1

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(30, result.Total);
        Assert.Equal(20, result.Results.Count());
    }
}
