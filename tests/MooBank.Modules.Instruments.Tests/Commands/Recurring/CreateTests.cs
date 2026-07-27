#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Commands.Recurring;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Recurring;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesRecurringTransaction()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = "Monthly Bills", Amount = 500m, Schedule = ScheduleFrequency.Monthly, NextRun = nextRun });

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Monthly Bills", result.Description);
        Assert.Equal(500m, result.Amount);
        Assert.Equal(ScheduleFrequency.Monthly, result.Schedule);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToVirtualInstrument()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = "Test", Amount = 100m, Schedule = ScheduleFrequency.Weekly, NextRun = nextRun });

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var virtualInstrument = account.VirtualInstruments.Single(v => v.Id == virtualId);
        Assert.Single(virtualInstrument.RecurringTransactions);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = "Test", Amount = 100m, Schedule = ScheduleFrequency.Weekly, NextRun = nextRun });

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidVirtualInstrumentId_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var wrongVirtualId = Guid.NewGuid();
        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, wrongVirtualId, new RecurringTransactionDetails { Description = "Test", Amount = 100m, Schedule = ScheduleFrequency.Weekly, NextRun = nextRun });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NullDescription_AllowsNullDescription()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = null, Amount = 100m, Schedule = ScheduleFrequency.Monthly, NextRun = nextRun });

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.Description);
    }

    [Theory]
    [InlineData(ScheduleFrequency.Daily)]
    [InlineData(ScheduleFrequency.Weekly)]
    [InlineData(ScheduleFrequency.Fortnightly)]
    [InlineData(ScheduleFrequency.Monthly)]
    [InlineData(ScheduleFrequency.Yearly)]
    public async Task Handle_DifferentSchedules_SetsCorrectSchedule(ScheduleFrequency schedule)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = "Test", Amount = 100m, Schedule = schedule, NextRun = nextRun });

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(schedule, result.Schedule);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsVirtualInstrumentId()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(id: virtualId, parentId: accountId);
        var account = TestEntities.CreateLogicalAccountWithVirtualInstruments(
            id: accountId,
            virtualInstruments: [vi]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<VirtualInstrumentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var nextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new Create(accountId, virtualId, new RecurringTransactionDetails { Description = "Test", Amount = 100m, Schedule = ScheduleFrequency.Monthly, NextRun = nextRun });

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(virtualId, result.VirtualInstrumentId);
    }
}
