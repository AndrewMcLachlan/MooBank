using Asm.MooBank.Queues;
using Asm.MooBank.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class ImportTransactionsBackgroundService(IImportTransactionsQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ImportTransactionsService>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IImportTransactionsQueue _taskQueue = taskQueue;

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Import Transactions Service is starting.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    // Set the user context for this scope
                    var userDataProvider = scope.ServiceProvider.GetRequiredService<ISettableUserDataProvider>();
                    userDataProvider.SetUser(workItem.User);

                    var importTransactionsService = scope.ServiceProvider.GetRequiredService<IImportTransactionsService>();

                    await importTransactionsService.Import(workItem, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred importing transactions for account {AccountId}.", workItem.AccountId);
                }
            }

            _logger.LogInformation("Import Transactions Service is stopping.");
        }
        catch (TaskCanceledException tcex)
        {
            _logger.LogWarning(tcex, "Import Transactions Service is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import Transactions Service encountered an error: {Message}", ex.Message);
        }
    }
}
