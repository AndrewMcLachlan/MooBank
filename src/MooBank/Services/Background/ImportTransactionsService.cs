using Asm.Hosting;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class ImportTransactionsBackgroundService(IBackgroundWorkQueue<ImportWorkItem> queue, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    : QueuedHostedService<ImportWorkItem>(queue, serviceScopeFactory, loggerFactory)
{
    protected override async ValueTask ProcessAsync(IServiceProvider services, ImportWorkItem workItem, CancellationToken cancellationToken)
    {
        // Set the user context for this scope before resolving the import service.
        services.GetRequiredService<ISettableUserDataProvider>().SetUser(workItem.User);

        var importTransactionsService = services.GetRequiredService<IImportTransactionsService>();
        await importTransactionsService.Import(workItem, cancellationToken);
    }
}
