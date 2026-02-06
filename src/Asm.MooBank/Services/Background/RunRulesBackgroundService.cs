using Asm.MooBank.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class RunRulesBackgroundService(IRunRulesQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RunRulesBackgroundService>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IRunRulesQueue _taskQueue = taskQueue;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Run Rules Service is starting.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var accountId = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var runRulesService = scope.ServiceProvider.GetRequiredService<IRunRulesService>();

                    await runRulesService.RunRules(accountId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred running rules for account {AccountId}.", accountId);
                }
            }

            _logger.LogInformation("Run Rules Service is stopping.");
        }
        catch (TaskCanceledException tcex)
        {
            _logger.LogWarning(tcex, "Run Rules Service is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run Rules Service encountered an error: {Message}", ex.Message);
        }
    }
}
