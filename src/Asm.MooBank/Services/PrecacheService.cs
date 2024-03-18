using System.Timers;
using Asm.MooBank.Domain.Entities.ReferenceData;
using LazyCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Timer = System.Timers.Timer;

namespace Asm.MooBank.Services;
public class PrecacheService : IHostedService
{
    private readonly Timer _timer = new(TimeSpan.FromMinutes(5).TotalMilliseconds);

    private readonly IAppCache appCache;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public PrecacheService(IAppCache appCache, IServiceScopeFactory serviceScopeFactory)
    {
        this.appCache = appCache;
        this.serviceScopeFactory = serviceScopeFactory;
        _timer.Elapsed += PingCache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();

        return Task.CompletedTask;
    }

    private void PingCache(object? sender, ElapsedEventArgs e)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var referenceDataRepository = scope.ServiceProvider.GetRequiredService<IReferenceDataRepository>();

        var _ = appCache.GetOrAddAsync($"IReferenceDataRepository-GetExchangeRates", referenceDataRepository.GetExchangeRates, DateTimeOffset.Now.AddHours(12)).Result;
    }
}
