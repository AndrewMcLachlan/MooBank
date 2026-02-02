#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Accounts;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Accounts;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(
            id: accountId,
            name: "Test Electricity",
            utilityType: UtilityType.Electricity,
            accountNumber: "ELEC123");
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.Id);
        Assert.Equal("Test Electricity", result.Name);
        Assert.Equal(UtilityType.Electricity, result.UtilityType);
    }

    [Fact]
    public async Task Handle_AccountWithBills_ReturnsBillDates()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var bills = new[]
        {
            TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 15)),
            TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 4, 15)),
            TestEntities.CreateBill(id: 3, issueDate: new DateOnly(2024, 7, 15)),
        };
        var account = TestEntities.CreateAccount(
            id: accountId,
            name: "Electricity",
            bills: bills);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(new DateOnly(2024, 1, 15), result.FirstBill);
        Assert.Equal(new DateOnly(2024, 7, 15), result.LatestBill);
    }

    [Fact]
    public async Task Handle_AccountWithoutBills_ReturnsNullBillDates()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);
        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result.FirstBill);
        Assert.Null(result.LatestBill);
    }

    [Fact]
    public async Task Handle_MultipleAccounts_ReturnsCorrectOne()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var accounts = new[]
        {
            TestEntities.CreateAccount(name: "First"),
            TestEntities.CreateAccount(id: targetId, name: "Target", utilityType: UtilityType.Gas),
            TestEntities.CreateAccount(name: "Third"),
        };
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var handler = new GetHandler(queryable);
        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target", result.Name);
        Assert.Equal(UtilityType.Gas, result.UtilityType);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accounts = TestEntities.CreateSampleAccounts();
        var queryable = TestEntities.CreateAccountQueryable(accounts);

        var handler = new GetHandler(queryable);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyCollection_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateAccountQueryable([]);

        var handler = new GetHandler(queryable);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }
}
