using Asm.MooBank.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class ReprocessTransactionsBackgroundService(IReprocessTransactionsQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ReprocessTransactionsBackgroundService>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IReprocessTransactionsQueue _taskQueue = taskQueue;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reprocess Transactions Service is starting.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var reprocessTransactionsService = scope.ServiceProvider.GetRequiredService<IReprocessTransactionsService>();

                    await reprocessTransactionsService.Reprocess(workItem, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred reprocessing transactions for account {AccountId}.", workItem.AccountId);
                }
            }

            _logger.LogInformation("Reprocess Transactions Service is stopping.");
        }
        catch (TaskCanceledException tcex)
        {
            _logger.LogWarning(tcex, "Reprocess Transactions Service is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reprocess Transactions Service encountered an error: {Message}", ex.Message);
        }
    }
}
