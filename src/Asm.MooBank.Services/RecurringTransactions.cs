using Asm.Domain;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public interface IRecurringTransactionService
{
    Task Process();
}

public class RecurringTransactionService : ServiceBase, IRecurringTransactionService
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<RecurringTransactionService> _logger;

    public RecurringTransactionService(IUnitOfWork unitOfWork, ITransactionService transactionService, IRecurringTransactionRepository recurringTransactionRepository, ILogger<RecurringTransactionService> logger) : base(unitOfWork)
    {
        _transactionService = transactionService;
        _recurringTransactionRepository = recurringTransactionRepository;
        _logger = logger;
    }

    public async Task Process()
    {
        foreach (var trans in await _recurringTransactionRepository.GetAll())
        {
            if (trans.LastRun == null)
            {
                _logger.LogInformation("Recurring transaction for {accountId} has not been run before. Creating first run.", trans.VirtualAccountId);
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
                    _logger.LogInformation("Running recurring transaction for {accountId}.", trans.VirtualAccountId);
                    RunTransaction(trans);
                    trans.LastRun = DateTime.Now;
                }
                else
                {
                    _logger.LogInformation("Recurring transaction for {accountId} not due to run", trans.VirtualAccountId);
                }
            }
        }

        await UnitOfWork.SaveChangesAsync();
    }

    private void RunTransaction(RecurringTransaction trans)
    {
        _transactionService.AddTransaction(trans.Amount, trans.VirtualAccountId, true, trans.Description);

        trans.VirtualAccount.Balance += trans.Amount;
        trans.VirtualAccount.LastUpdated = DateTime.UtcNow;
    }
}