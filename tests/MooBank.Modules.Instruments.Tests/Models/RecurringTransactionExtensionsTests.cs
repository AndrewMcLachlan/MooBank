#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using DomainRecurringTransaction = Asm.MooBank.Domain.Entities.Instrument.RecurringTransaction;

namespace Asm.MooBank.Modules.Instruments.Tests.Models;

/// <summary>
/// Unit tests for mapping a recurring transaction to its model.
/// </summary>
[Trait("Category", "Unit")]
public class RecurringTransactionExtensionsTests
{
    /// <summary>
    /// Given a LastRun read back from the database with an unspecified kind
    /// When the entity is mapped to its model
    /// Then LastRun should carry a zero (UTC) offset and the same clock reading
    /// </summary>
    /// <remarks>
    /// The service writes LastRun as <see cref="DateTime.UtcNow"/> into a DATETIME2 column, and
    /// EF returns it as <see cref="DateTimeKind.Unspecified"/>. Converting that to a
    /// DateTimeOffset implicitly applies the host's local offset, which shifts the value.
    /// This test only fails on a host that is not already on UTC.
    /// </remarks>
    [Fact]
    public void ToModel_LastRunFromDatabase_IsTreatedAsUtc()
    {
        // Arrange
        var storedLastRun = new DateTime(2026, 1, 15, 22, 30, 0, DateTimeKind.Unspecified);

        var entity = new DomainRecurringTransaction(Guid.NewGuid())
        {
            VirtualInstrumentId = Guid.NewGuid(),
            Amount = 100m,
            Schedule = ScheduleFrequency.Monthly,
            NextRun = new DateOnly(2026, 2, 15),
            LastRun = storedLastRun,
        };

        // Act
        var model = entity.ToModel();

        // Assert
        Assert.NotNull(model.LastRun);
        Assert.Equal(TimeSpan.Zero, model.LastRun.Value.Offset);
        Assert.Equal(storedLastRun, model.LastRun.Value.UtcDateTime);
    }

    /// <summary>
    /// Given a recurring transaction that has never run
    /// When the entity is mapped to its model
    /// Then LastRun should be null
    /// </summary>
    [Fact]
    public void ToModel_NeverRun_LastRunIsNull()
    {
        // Arrange
        var entity = new DomainRecurringTransaction(Guid.NewGuid())
        {
            VirtualInstrumentId = Guid.NewGuid(),
            Amount = 100m,
            Schedule = ScheduleFrequency.Monthly,
            NextRun = new DateOnly(2026, 2, 15),
            LastRun = null,
        };

        // Act
        var model = entity.ToModel();

        // Assert
        Assert.Null(model.LastRun);
    }
}
