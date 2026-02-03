#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands.Recurring;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands.Recurring;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesRecurringTransaction()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            description: "Old Description",
            amount: 100m,
            schedule: ScheduleFrequency.Weekly);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var newNextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var command = new Update(accountId, virtualId, rtId, "New Description", 200m, ScheduleFrequency.Monthly, newNextRun);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Description", result.Description);
        Assert.Equal(200m, result.Amount);
        Assert.Equal(ScheduleFrequency.Monthly, result.Schedule);
        Assert.Equal(newNextRun, result.NextRun);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
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

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Update(accountId, virtualId, rtId, "Updated", 150m, ScheduleFrequency.Weekly, nextRun);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidVirtualAccountId_ThrowsNotFoundException()
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

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var wrongVirtualId = Guid.NewGuid();
        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Update(accountId, wrongVirtualId, rtId, "Test", 100m, ScheduleFrequency.Weekly, nextRun);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_InvalidRecurringTransactionId_ThrowsNotFoundException()
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

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var wrongRtId = Guid.NewGuid();
        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Update(accountId, virtualId, wrongRtId, "Test", 100m, ScheduleFrequency.Weekly, nextRun);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_UpdateToNullDescription_AllowsNullDescription()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            description: "Original Description");

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Update(accountId, virtualId, rtId, null, 100m, ScheduleFrequency.Monthly, nextRun);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task Handle_UpdateAmount_UpdatesEntityAmount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt = TestEntities.CreateRecurringTransaction(
            id: rtId,
            virtualAccountId: virtualId,
            amount: 100m);

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt]);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Update(accountId, virtualId, rtId, "Test", 999.99m, ScheduleFrequency.Monthly, nextRun);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedRt = account.VirtualInstruments
            .Single(v => v.Id == virtualId).RecurringTransactions
            .Single(r => r.Id == rtId);
        Assert.Equal(999.99m, updatedRt.Amount);
    }
}
