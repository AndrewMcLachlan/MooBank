using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

/// <summary>
/// Interface for the recurring transaction service.
/// </summary>
public interface IRecurringTransactionService
{
    Task Process();
}

/// <summary>
/// Processes recurring transactions.
/// </summary>
public class RecurringTransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IRecurringTransactionRepository recurringTransactionRepository, ILogger<RecurringTransactionService> logger) : IRecurringTransactionService
{
    /// <summary>
    /// Get all recurring transactions and process them.
    /// </summary>
    /// <returns>A task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the schedule type is unrecognised.</exception>
    public async Task Process()
    {
        foreach (var trans in await recurringTransactionRepository.Get())
        {
            while (trans.NextRun <= DateTime.UtcNow.ToDateOnly())
            {
                logger.LogInformation("Running recurring transaction for {accountId}.", trans.VirtualAccountId);
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

        await unitOfWork.SaveChangesAsync();
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
            "Recurring"
        );

        transaction.PurchaseDate = recurring.NextRun.ToDateTime(TimeOnly.MinValue);

        transactionRepository.Add(transaction);
    }
}
