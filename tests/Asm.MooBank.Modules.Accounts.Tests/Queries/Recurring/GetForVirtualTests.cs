#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries.Recurring;

[Trait("Category", "Unit")]
public class GetForVirtualTests
{
    [Fact]
    public async Task Handle_ValidVirtualAccount_ReturnsRecurringTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId, description: "RT1");
        var rt2 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId, description: "RT2");

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt1, rt2]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(accountId, virtualId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Description == "RT1");
        Assert.Contains(result, r => r.Description == "RT2");
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(Guid.NewGuid(), virtualId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentVirtualAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(accountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NoRecurringTransactions_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(accountId, virtualId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MultipleVirtualInstruments_ReturnsOnlySpecifiedVirtualAccountTransactions()
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

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(accountId, virtualId1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Description == "VI1 RT1");
        Assert.Contains(result, r => r.Description == "VI1 RT2");
        Assert.DoesNotContain(result, r => r.Description == "VI2 RT1");
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
            amount: 200m,
            schedule: MooBank.Models.ScheduleFrequency.Yearly,
            nextRun: nextRun);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        var queryable = TestEntities.CreateInstrumentQueryable(account);

        var handler = new GetForVirtualHandler(queryable);
        var query = new GetForVirtual(accountId, virtualId);

        // Act
        var result = (await handler.Handle(query, TestContext.Current.CancellationToken)).Single();

        // Assert
        Assert.Equal(rtId, result.Id);
        Assert.Equal(virtualId, result.VirtualAccountId);
        Assert.Equal("Complete RT", result.Description);
        Assert.Equal(200m, result.Amount);
        Assert.Equal(MooBank.Models.ScheduleFrequency.Yearly, result.Schedule);
        Assert.Equal(nextRun, result.NextRun);
    }

    [Fact]
    public async Task Handle_VirtualAccountFromDifferentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var virtualId1 = Guid.NewGuid();
        var virtualId2 = Guid.NewGuid();

        var vi1 = TestEntities.CreateVirtualInstrument(id: virtualId1, parentId: accountId1);
        var vi2 = TestEntities.CreateVirtualInstrument(id: virtualId2, parentId: accountId2);

        var account1 = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId1,
            virtualInstruments: [vi1]);
        var account2 = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId2,
            virtualInstruments: [vi2]);

        var queryable = TestEntities.CreateInstrumentQueryable(account1, account2);

        var handler = new GetForVirtualHandler(queryable);
        // Request virtual account from account2 but specify account1 as the parent
        var query = new GetForVirtual(accountId1, virtualId2);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }
}
