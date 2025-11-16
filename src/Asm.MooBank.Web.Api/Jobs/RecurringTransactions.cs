using Asm.MooBank.Services;
using Microsoft.Azure.WebJobs;

namespace Asm.MooBank.Web.Api.Jobs;

public class RecurringTransactions(IServiceScopeFactory serviceScopeFactory)
{
#if DEBUG
    private const bool RunOnStartup = true;
#else
    private const bool RunOnStartup = false;
#endif

    [FunctionName("RecurringTransactions")]
    public async Task Run([TimerTrigger("0 0 14 * * *", RunOnStartup = RunOnStartup)] TimerInfo _)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();
        await service.Process();
    }
}
