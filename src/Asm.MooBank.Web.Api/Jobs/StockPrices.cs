using Microsoft.Azure.WebJobs;

namespace Asm.MooBank.Web.Api.Jobs;

public class StockPrices(IServiceScopeFactory serviceScopeFactory)
{
#if DEBUG
    private const bool RunOnStartup = true;
#else
    private const bool RunOnStartup = false;
#endif

    [FunctionName("StockPrices")]
    public async Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = RunOnStartup)] TimerInfo _)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<Asm.MooBank.Services.IStockPriceService>();
        await service.Update();
    }
}
