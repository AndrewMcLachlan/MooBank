#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries.Recurring;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ExistingRecurringTransaction_ReturnsTransaction()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            description: "Monthly Bill",
            amount: 100m);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, rtId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rtId, result.Id);
        Assert.Equal("Monthly Bill", result.Description);
        Assert.Equal(100m, result.Amount);
    }

    [Fact]
    public async Task Handle_NonExistentRecurringTransaction_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt = TestEntities.CreateRecurringTransaction(id: rtId, virtualAccountId: virtualId);
        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleRecurringTransactions_ReturnsCorrectOne()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId1 = Guid.NewGuid();
        var rtId2 = Guid.NewGuid();
        var rtId3 = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(id: rtId1, virtualAccountId: virtualId, description: "First");
        var rt2 = TestEntities.CreateRecurringTransaction(id: rtId2, virtualAccountId: virtualId, description: "Second");
        var rt3 = TestEntities.CreateRecurringTransaction(id: rtId3, virtualAccountId: virtualId, description: "Third");

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt1, rt2, rt3]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, rtId2);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Second", result.Description);
    }

    [Fact]
    public async Task Handle_RecurringTransactionInDifferentVirtualInstruments_FindsCorrectOne()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId1 = Guid.NewGuid();
        var virtualId2 = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId1, description: "VI1 RT");
        var rt2 = TestEntities.CreateRecurringTransaction(id: rtId, virtualAccountId: virtualId2, description: "Target RT");

        var vi1 = TestEntities.CreateVirtualInstrument(
            id: virtualId1,
            parentId: accountId,
            recurringTransactions: [rt1]);
        var vi2 = TestEntities.CreateVirtualInstrument(
            id: virtualId2,
            parentId: accountId,
            recurringTransactions: [rt2]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi1, vi2]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, rtId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Target RT", result.Description);
        Assert.Equal(virtualId2, result.VirtualAccountId);
    }

    [Fact]
    public async Task Handle_ReturnsAllProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();
        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var lastRun = DateTime.UtcNow.AddDays(-7);

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            description: "Complete RT",
            amount: 250.50m,
            schedule: MooBank.Models.ScheduleFrequency.Fortnightly,
            nextRun: nextRun,
            lastRun: lastRun);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetHandler(queryable);
        var query = new Get(accountId, rtId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(rtId, result.Id);
        Assert.Equal(virtualId, result.VirtualAccountId);
        Assert.Equal("Complete RT", result.Description);
        Assert.Equal(250.50m, result.Amount);
        Assert.Equal(MooBank.Models.ScheduleFrequency.Fortnightly, result.Schedule);
        Assert.Equal(nextRun, result.NextRun);
    }
}
