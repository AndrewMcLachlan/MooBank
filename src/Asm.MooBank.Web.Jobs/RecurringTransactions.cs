using Asm.MooBank.Services;
using Microsoft.Azure.WebJobs;

namespace Asm.MooBank.Web.Jobs;
public class RecurringTransactions(IRecurringTransactionService recurringTransactionService)
{
#if DEBUG
    private const bool RunOnStartup = true;
#else
    private const bool RunOnStartup = false;
#endif

    public Task Run([TimerTrigger("0 0 14 * * *", RunOnStartup = RunOnStartup)] TimerInfo _) =>
        recurringTransactionService.Process();
}
