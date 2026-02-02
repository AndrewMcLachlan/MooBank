#nullable enable
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Bills;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ExistingBill_ReturnsBill()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill(
            id: 1,
            invoiceNumber: "INV001",
            issueDate: new DateOnly(2024, 5, 15),
            cost: 150.50m);
        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("INV001", result.InvoiceNumber);
        Assert.Equal(new DateOnly(2024, 5, 15), result.IssueDate);
        Assert.Equal(150.50m, result.Cost);
    }

    [Fact]
    public async Task Handle_MultipleBills_ReturnsCorrectOne()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = new[]
        {
            TestEntities.CreateBill(id: 1, invoiceNumber: "INV001"),
            TestEntities.CreateBill(id: 2, invoiceNumber: "INV002"),
            TestEntities.CreateBill(id: 3, invoiceNumber: "INV003"),
        };
        var account = TestEntities.CreateAccount(id: accountId, bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Id);
        Assert.Equal("INV002", result.InvoiceNumber);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var account = TestEntities.CreateAccount();
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(Guid.NewGuid(), 1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentBill_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill(id: 1);
        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_AccountWithNoBills_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_BillWithPeriods_ReturnsPeriods()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var periods = new[]
        {
            TestEntities.CreatePeriod(id: 1, periodStart: new DateTime(2024, 4, 1), periodEnd: new DateTime(2024, 4, 30)),
            TestEntities.CreatePeriod(id: 2, periodStart: new DateTime(2024, 5, 1), periodEnd: new DateTime(2024, 5, 31)),
        };
        var bill = TestEntities.CreateBill(id: 1, periods: periods);
        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Periods);
        Assert.Equal(2, result.Periods.Count());
    }

    [Fact]
    public async Task Handle_BillWithDiscounts_ReturnsDiscounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var discounts = new[]
        {
            TestEntities.CreateDiscount(id: 1, discountPercent: 10, reason: "Early payment"),
        };
        var bill = TestEntities.CreateBill(id: 1, discounts: discounts);
        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, 1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Discounts);
        Assert.Single(result.Discounts);
    }
}
