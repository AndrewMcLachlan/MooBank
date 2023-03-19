using Asm.Domain;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services;

public interface IRecurringTransactionService
{
    Task Process();
}

public class RecurringTransactionService : ServiceBase, IRecurringTransactionService
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly ITransactionService _transactionService;

    public RecurringTransactionService(IUnitOfWork unitOfWork, ITransactionService transactionService, IRecurringTransactionRepository recurringTransactionRepository) : base(unitOfWork)
    {
        _transactionService = transactionService;
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task Process()
    {
        foreach (var trans in await _recurringTransactionRepository.GetAll())
        {
            if (trans.LastRun == null)
            {
                await RunTransaction(trans);
                trans.LastRun = DateTime.Now;
            }
            else
            {
                DateTime lastRun = trans.LastRun.Value;
                DateTime now = DateTime.Now;

                TimeSpan diff = now - lastRun;

                bool process = false;
                switch (trans.Schedule)
                {
                    case ScheduleFrequency.Daily:
                        process = diff.TotalDays >= 1;
                        break;
                    case ScheduleFrequency.Weekly:
                        process = diff.TotalDays >= 7;
                        break;
                    case ScheduleFrequency.Monthly:
                        // Make sure some time has passed and
                        // that we are one calendar month apart or as close as we can be
                        // (e.g. if the transaction last ran on the 31st Jan, the next run will be the 28th Feb).

                        process = diff.TotalDays >= 28 && (lastRun.Day == now.Day || DateTime.DaysInMonth(now.Year, now.Month) < lastRun.Day);
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported schedule: " + trans.Schedule.ToString());
                }

                if (process)
                {
                    await RunTransaction(trans);
                    trans.LastRun = DateTime.Now;
                }
            }
        }

        await UnitOfWork.SaveChangesAsync();
    }

    private async Task RunTransaction(RecurringTransaction trans)
    {
        await _transactionService.AddTransaction(trans.Amount, trans.VirtualAccountId, true, trans.Description);

        trans.VirtualAccount.Balance += trans.Amount;
    }
}