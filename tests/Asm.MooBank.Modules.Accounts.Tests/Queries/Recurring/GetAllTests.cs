#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries.Recurring;

[Trait("Category", "Unit")]
public class GetAllTests
{
    [Fact]
    public async Task Handle_NoRecurringTransactions_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_SingleVirtualInstrument_ReturnsAllRecurringTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId, description: "RT1");
        var rt2 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId, description: "RT2");
        var rt3 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId, description: "RT3");

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt1, rt2, rt3]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(result, r => r.Description == "RT1");
        Assert.Contains(result, r => r.Description == "RT2");
        Assert.Contains(result, r => r.Description == "RT3");
    }

    [Fact]
    public async Task Handle_MultipleVirtualInstruments_ReturnsAllRecurringTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId1 = Guid.NewGuid();
        var virtualId2 = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId1, description: "VI1 RT1");
        var rt2 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId1, description: "VI1 RT2");
        var rt3 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId2, description: "VI2 RT1");

        var vi1 = TestEntities.CreateVirtualInstrument(
            id: virtualId1,
            parentId: accountId,
            recurringTransactions: [rt1, rt2]);
        var vi2 = TestEntities.CreateVirtualInstrument(
            id: virtualId2,
            parentId: accountId,
            recurringTransactions: [rt3]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi1, vi2]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(result, r => r.Description == "VI1 RT1");
        Assert.Contains(result, r => r.Description == "VI1 RT2");
        Assert.Contains(result, r => r.Description == "VI2 RT1");
    }

    [Fact]
    public async Task Handle_NoVirtualInstruments_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(id: accountId);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsAllProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();
        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            description: "Complete RT",
            amount: 150m,
            schedule: MooBank.Models.ScheduleFrequency.Monthly,
            nextRun: nextRun);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId);

        // Act
        var result = (await handler.Handle(query, TestContext.Current.CancellationToken)).Single();

        // Assert
        Assert.Equal(rtId, result.Id);
        Assert.Equal(virtualId, result.VirtualAccountId);
        Assert.Equal("Complete RT", result.Description);
        Assert.Equal(150m, result.Amount);
        Assert.Equal(MooBank.Models.ScheduleFrequency.Monthly, result.Schedule);
        Assert.Equal(nextRun, result.NextRun);
    }

    [Fact]
    public async Task Handle_DifferentAccounts_OnlyReturnsRecurringTransactionsForRequestedAccount()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var virtualId1 = Guid.NewGuid();
        var virtualId2 = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId1, description: "Account1 RT");
        var rt2 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId2, description: "Account2 RT");

        var vi1 = TestEntities.CreateVirtualInstrument(
            id: virtualId1,
            parentId: accountId1,
            recurringTransactions: [rt1]);
        var vi2 = TestEntities.CreateVirtualInstrument(
            id: virtualId2,
            parentId: accountId2,
            recurringTransactions: [rt2]);

        var account1 = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId1,
            virtualInstruments: [vi1]);
        var account2 = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId2,
            virtualInstruments: [vi2]);

        var queryable = TestEntities.CreateInstrumentQueryable(account1, account2);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountId1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Account1 RT", result.First().Description);
    }
}
