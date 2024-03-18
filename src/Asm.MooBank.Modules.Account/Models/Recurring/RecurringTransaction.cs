using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Account.Models.Recurring;
public record RecurringTransaction
{
    public Guid Id { get; init; }

    public Guid VirtualAccountId { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }

    public DateTimeOffset? LastRun { get; init; }

    public DateOnly NextRun { get; init; }

    public ScheduleFrequency Schedule { get; init; }
}

public static class RecurringTransactionExtensions
{
    public static RecurringTransaction ToModel(this Domain.Entities.Account.RecurringTransaction recurringTransaction) =>
        new()
        {
            Description = recurringTransaction.Description,
            Amount = recurringTransaction.Amount,
            LastRun = recurringTransaction.LastRun,
            NextRun = recurringTransaction.NextRun,
            Schedule = recurringTransaction.Schedule,
            Id = recurringTransaction.Id,
            VirtualAccountId = recurringTransaction.VirtualAccountId,
        };

    public static IEnumerable<RecurringTransaction> ToModel(this IEnumerable<Domain.Entities.Account.RecurringTransaction> recurringTransactions) =>
        recurringTransactions.Select(t => t.ToModel());
}
