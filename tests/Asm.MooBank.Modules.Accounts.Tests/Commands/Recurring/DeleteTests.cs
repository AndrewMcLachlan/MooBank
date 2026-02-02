#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Modules.Accounts.Commands.Recurring;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands.Recurring;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesRecurringTransaction()
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
        // Set the VirtualAccount navigation property
        rt.VirtualAccount = vi;

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(accountId, rtId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var virtualInstrument = account.VirtualInstruments.Single(v => v.Id == virtualId);
        Assert.Empty(virtualInstrument.RecurringTransactions);
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
        rt.VirtualAccount = vi;

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(accountId, rtId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        rt.VirtualAccount = vi;

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var wrongRtId = Guid.NewGuid();
        var command = new Delete(accountId, wrongRtId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleRecurringTransactions_DeletesCorrectOne()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var rtId1 = Guid.NewGuid();
        var rtId2 = Guid.NewGuid();
        var rtId3 = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(id: rtId1, virtualAccountId: virtualId, description: "Keep 1");
        var rt2 = TestEntities.CreateRecurringTransaction(id: rtId2, virtualAccountId: virtualId, description: "Delete Me");
        var rt3 = TestEntities.CreateRecurringTransaction(id: rtId3, virtualAccountId: virtualId, description: "Keep 2");

        var vi = TestEntities.CreateVirtualInstrument(
            id: virtualId,
            parentId: accountId,
            recurringTransactions: [rt1, rt2, rt3]);

        rt1.VirtualAccount = vi;
        rt2.VirtualAccount = vi;
        rt3.VirtualAccount = vi;

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(accountId, rtId2);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var virtualInstrument = account.VirtualInstruments.Single(v => v.Id == virtualId);
        Assert.Equal(2, virtualInstrument.RecurringTransactions.Count);
        Assert.Contains(virtualInstrument.RecurringTransactions, r => r.Description == "Keep 1");
        Assert.Contains(virtualInstrument.RecurringTransactions, r => r.Description == "Keep 2");
        Assert.DoesNotContain(virtualInstrument.RecurringTransactions, r => r.Description == "Delete Me");
    }

    [Fact]
    public async Task Handle_MultipleVirtualInstruments_FindsCorrectRecurringTransaction()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId1 = Guid.NewGuid();
        var virtualId2 = Guid.NewGuid();
        var rtId = Guid.NewGuid();

        var rt1 = TestEntities.CreateRecurringTransaction(virtualAccountId: virtualId1, description: "VI1 RT");
        var rt2 = TestEntities.CreateRecurringTransaction(id: rtId, virtualAccountId: virtualId2, description: "Delete Me");

        var vi1 = TestEntities.CreateVirtualInstrument(
            id: virtualId1,
            parentId: accountId,
            recurringTransactions: [rt1]);
        var vi2 = TestEntities.CreateVirtualInstrument(
            id: virtualId2,
            parentId: accountId,
            recurringTransactions: [rt2]);

        rt1.VirtualAccount = vi1;
        rt2.VirtualAccount = vi2;

        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi1, vi2]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<RecurringTransactionSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(accountId, rtId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var virtualInstrument1 = account.VirtualInstruments.Single(v => v.Id == virtualId1);
        var virtualInstrument2 = account.VirtualInstruments.Single(v => v.Id == virtualId2);
        Assert.Single(virtualInstrument1.RecurringTransactions);
        Assert.Empty(virtualInstrument2.RecurringTransactions);
    }
}
