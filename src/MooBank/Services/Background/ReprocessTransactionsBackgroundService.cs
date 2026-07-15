using Asm.Hosting;
using Asm.MooBank.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class ReprocessTransactionsBackgroundService(IBackgroundWorkQueue<ReprocessWorkItem> queue, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    : QueuedHostedService<ReprocessWorkItem>(queue, serviceScopeFactory, loggerFactory)
{
    protected override async ValueTask ProcessAsync(IServiceProvider services, ReprocessWorkItem workItem, CancellationToken cancellationToken)
    {
        var reprocessTransactionsService = services.GetRequiredService<IReprocessTransactionsService>();
        await reprocessTransactionsService.Reprocess(workItem, cancellationToken);
    }
}
