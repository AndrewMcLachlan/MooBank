using Asm.MooBank.Domain.Entities.ReferenceData;
using LazyCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asm.MooBank.Services.Background;

public class PrecacheService(IAppCache appCache, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await PingCache(stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing
        }
    }

    private async Task PingCache(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var referenceDataRepository = scope.ServiceProvider.GetRequiredService<IReferenceDataRepository>();

        var _ = await appCache.GetOrAddAsync($"IReferenceDataRepository-GetExchangeRates", () => referenceDataRepository.GetExchangeRates(cancellationToken), DateTimeOffset.Now.AddHours(12));
    }
}
