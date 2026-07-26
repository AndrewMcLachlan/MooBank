using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

/// <summary>
/// Interface for the recurring transaction service.
/// </summary>
public interface IRecurringTransactionService
{
    Task Process(CancellationToken cancellationToken = default);
}

/// <summary>
/// Processes recurring transactions.
/// </summary>
public class RecurringTransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IInstrumentRepository instrumentRepository, ILogger<RecurringTransactionService> logger) : IRecurringTransactionService
{
    /// <summary>
    /// Get all recurring transactions and process them.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task Process(CancellationToken cancellationToken = default)
    {
        // Background path: loads across all users via the unfiltered specification overload.
        var instruments = await instrumentRepository.Get(new VirtualInstrumentSpecification(), cancellationToken);

        foreach (var recurring in instruments.SelectMany(i => i.VirtualInstruments).SelectMany(v => v.RecurringTransactions))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ProcessDueOccurrences(recurring);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // This job runs across every user, so one bad recurring transaction must not
                // stop the rest from running. Log it and carry on; whatever succeeded before
                // the failure is still saved below.
                logger.LogError(ex, "Failed to process recurring transaction {RecurringTransactionId} for virtual instrument {VirtualInstrumentId}.", recurring.Id, recurring.VirtualInstrumentId);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Run a recurring transaction for every occurrence that is now due, catching up if the
    /// job has not run for a while.
    /// </summary>
    /// <param name="recurring">The recurring transaction definition.</param>
    /// <exception cref="InvalidOperationException">Thrown when the schedule type is unrecognised.</exception>
    private void ProcessDueOccurrences(RecurringTransaction recurring)
    {
        while (recurring.NextRun <= DateTime.UtcNow.ToDateOnly())
        {
            // Resolve the following occurrence first: an unrecognised schedule then throws
            // before anything is mutated, rather than leaving a transaction created against a
            // NextRun that never advanced (which would replay it on the next run).
            var nextRun = NextRun(recurring.NextRun, recurring.Schedule);

            logger.LogInformation("Running recurring transaction for {VirtualInstrumentId}.", recurring.VirtualInstrumentId);

            RunTransaction(recurring);

            recurring.LastRun = DateTime.UtcNow;
            recurring.NextRun = nextRun;
        }
    }

    /// <summary>
    /// Work out when a schedule next falls due after the given occurrence.
    /// </summary>
    /// <param name="occurrence">The occurrence being run.</param>
    /// <param name="schedule">The schedule to advance.</param>
    /// <returns>The date of the following occurrence.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the schedule type is unrecognised.</exception>
    private static DateOnly NextRun(DateOnly occurrence, ScheduleFrequency schedule) => schedule switch
    {
        ScheduleFrequency.Daily => occurrence.AddDays(1),
        ScheduleFrequency.Weekly => occurrence.AddDays(7),
        ScheduleFrequency.Fortnightly => occurrence.AddDays(14),
        ScheduleFrequency.Monthly => occurrence.AddMonths(1),
        ScheduleFrequency.Yearly => occurrence.AddYears(1),
        _ => throw new InvalidOperationException($"Unsupported schedule: {schedule}"),
    };

    /// <summary>
    /// Execute the transaction and update the balance.
    /// </summary>
    /// <param name="recurring">The recurring transaction definition.</param>
    private void RunTransaction(RecurringTransaction recurring)
    {
        var transaction = Domain.Entities.Transactions.Transaction.Create(
            recurring.VirtualInstrumentId,
            null,
            recurring.Amount,
            recurring.Description,
            // UTC, matching LastRun above and the other programmatic transaction creators.
            DateTime.UtcNow,
            TransactionSubType.Recurring,
            "Recurring",
            null
        );

        transaction.PurchaseDate = recurring.NextRun.ToDateTime(TimeOnly.MinValue);

        transactionRepository.Add(transaction);
    }
}
