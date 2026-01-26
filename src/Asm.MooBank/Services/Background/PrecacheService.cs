using Asm.MooBank.Domain.Entities.ReferenceData;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asm.MooBank.Services.Background;

public class PrecacheService(HybridCache cache, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly static HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(12),
    };

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

        var _ = await cache.GetOrCreateAsync(
            CacheKeys.ReferenceData.ExchangeRates, 
            async ct => await referenceDataRepository.GetExchangeRates(ct), CacheOptions,
            [CacheKeys.ReferenceData.CacheTag],
            cancellationToken);
    }
}
