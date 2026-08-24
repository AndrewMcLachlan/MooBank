using Asm.MooBank.Services.DemoData;
using Microsoft.Azure.WebJobs;

namespace Asm.MooBank.Api.Jobs;

public class DemoData(IServiceScopeFactory serviceScopeFactory)
{
#if DEBUG
    private const bool RunOnStartup = true;
#else
    private const bool RunOnStartup = false;
#endif

    /// <summary>
    /// Adds the month just ended to the demo instruments, at 02:00 on the first of each month.
    /// </summary>
    /// <remarks>
    /// An hour clear of the daily reference-data jobs at midnight. The service writes nothing
    /// unless instruments are configured, so this is inert in every environment that has not named
    /// them.
    /// </remarks>
    [FunctionName("DemoData")]
    public async Task Run([TimerTrigger("0 0 2 1 * *", RunOnStartup = RunOnStartup)] TimerInfo _, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDemoDataService>();
        await service.Extend(cancellationToken);
    }
}
