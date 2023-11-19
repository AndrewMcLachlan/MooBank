using Asm.Domain;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

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
public class RecurringTransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IRecurringTransactionRepository recurringTransactionRepository, ILogger<RecurringTransactionService> logger) : ServiceBase(unitOfWork), IRecurringTransactionService
{
    /// <summary>
    /// Get all recurring transactions and process them.
    /// </summary>
    /// <returns>A task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the schedule type is unrecognised.</exception>
    public async Task Process()
    {
        foreach (var trans in await recurringTransactionRepository.GetAll())
        {
            if (trans.LastRun == null)
            {
                logger.LogInformation("Recurring transaction for {accountId} has not been run before. Creating first run.", trans.VirtualAccountId);
                RunTransaction(trans);
                trans.LastRun = DateTime.Now;
            }
            else
            {
                DateTime lastRun = trans.LastRun.Value.Date;
                DateTime now = DateTime.Today;

                TimeSpan diff = now - lastRun;

                bool process = false;
                process = trans.Schedule switch
                {
                    ScheduleFrequency.Daily => diff.TotalDays >= 1,
                    ScheduleFrequency.Weekly => diff.TotalDays >= 7,

                    // Make sure some time has passed and
                    // that we are one calendar month apart or as close as we can be
                    // (e.g. if the transaction last ran on the 31st Jan, the next run will be the 28th Feb).
                    ScheduleFrequency.Monthly => diff.TotalDays >= 28 && (lastRun.Day == now.Day || DateTime.DaysInMonth(now.Year, now.Month) < lastRun.Day),
                    _ => throw new InvalidOperationException("Unsupported schedule: " + trans.Schedule.ToString()),
                };
                if (process)
                {
                    logger.LogInformation("Running recurring transaction for {accountId}.", trans.VirtualAccountId);
                    RunTransaction(trans);
                    trans.LastRun = DateTime.Now;
                }
                else
                {
                    logger.LogInformation("Recurring transaction for {accountId} not due to run", trans.VirtualAccountId);
                }
            }
        }

        await UnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Execute the transaction and update the balance.
    /// </summary>
    /// <param name="trans">The recurring transaction definition.</param>
    private void RunTransaction(RecurringTransaction trans)
    {
        TransactionType transactionType = trans.Amount < 0 ?
                                  TransactionType.RecurringDebit:
                                  TransactionType.RecurringCredit;

        Domain.Entities.Transactions.Transaction transaction = new()
        {
            Amount = trans.Amount,
            AccountId = trans.VirtualAccountId,
            Description = trans.Description,
            Source = "Recurring",
            TransactionTime = DateTime.Now,
            TransactionType = transactionType,
        };

        transactionRepository.Add(transaction);

        trans.VirtualAccount.Balance += trans.Amount;
        trans.VirtualAccount.LastUpdated = DateTime.UtcNow;
    }
}