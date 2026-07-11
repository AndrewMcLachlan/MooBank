using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
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
    /// <exception cref="InvalidOperationException">Thrown when the schedule type is unrecognised.</exception>
    public async Task Process(CancellationToken cancellationToken = default)
    {
        // Background path: loads across all users via the unfiltered specification overload.
        var instruments = await instrumentRepository.Get(new RecurringTransactionSpecification(), cancellationToken);

        foreach (var trans in instruments.SelectMany(i => i.VirtualInstruments).SelectMany(v => v.RecurringTransactions))
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (trans.NextRun <= DateTime.UtcNow.ToDateOnly())
            {
                logger.LogInformation("Running recurring transaction for {AccountId}.", trans.VirtualAccountId);
                RunTransaction(trans);
                trans.LastRun = DateTime.UtcNow;
                trans.NextRun = trans.Schedule switch
                {
                    ScheduleFrequency.Daily => trans.NextRun.AddDays(1),
                    ScheduleFrequency.Weekly => trans.NextRun.AddDays(7),
                    ScheduleFrequency.Monthly => trans.NextRun.AddMonths(1),
                    _ => throw new InvalidOperationException("Unsupported schedule: " + trans.Schedule.ToString()),
                };
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Execute the transaction and update the balance.
    /// </summary>
    /// <param name="recurring">The recurring transaction definition.</param>
    private void RunTransaction(RecurringTransaction recurring)
    {
        var transaction = Domain.Entities.Transactions.Transaction.Create(
            recurring.VirtualAccountId,
            null,
            recurring.Amount,
            recurring.Description,
            DateTime.Now,
            TransactionSubType.Recurring,
            "Recurring",
            null
        );

        transaction.PurchaseDate = recurring.NextRun.ToDateTime(TimeOnly.MinValue);

        transactionRepository.Add(transaction);
    }
}
