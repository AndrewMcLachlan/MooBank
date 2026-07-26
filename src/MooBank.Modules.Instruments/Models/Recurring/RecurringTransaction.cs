using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Recurring;

public record RecurringTransaction
{
    public Guid Id { get; init; }

    public Guid VirtualInstrumentId { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }

    public DateTimeOffset? LastRun { get; init; }

    public DateOnly NextRun { get; init; }

    public ScheduleFrequency Schedule { get; init; }
}

public static class RecurringTransactionExtensions
{
    public static RecurringTransaction ToModel(this Domain.Entities.Instrument.RecurringTransaction recurringTransaction) =>
        new()
        {
            Description = recurringTransaction.Description,
            Amount = recurringTransaction.Amount,
            LastRun = AsUtc(recurringTransaction.LastRun),
            NextRun = recurringTransaction.NextRun,
            Schedule = recurringTransaction.Schedule,
            Id = recurringTransaction.Id,
            VirtualInstrumentId = recurringTransaction.VirtualInstrumentId,
        };

    public static IEnumerable<RecurringTransaction> ToModel(this IEnumerable<Domain.Entities.Instrument.RecurringTransaction> recurringTransactions) =>
        recurringTransactions.Select(t => t.ToModel());

    /// <summary>
    /// LastRun is written as UTC but stored in a DATETIME2 column, which EF reads back as
    /// <see cref="DateTimeKind.Unspecified"/>. Converting that to a DateTimeOffset implicitly
    /// would stamp the server's local offset onto a UTC value, so the kind is restored first.
    /// </summary>
    private static DateTimeOffset? AsUtc(DateTime? value) =>
        value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
