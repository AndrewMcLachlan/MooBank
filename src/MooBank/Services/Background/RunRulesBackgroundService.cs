using Asm.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Background;

internal class RunRulesBackgroundService(IBackgroundWorkQueue<Guid> queue, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    : QueuedHostedService<Guid>(queue, serviceScopeFactory, loggerFactory)
{
    protected override async ValueTask ProcessAsync(IServiceProvider services, Guid accountId, CancellationToken cancellationToken)
    {
        var runRulesService = services.GetRequiredService<IRunRulesService>();
        await runRulesService.RunRules(accountId, cancellationToken);
    }

    protected override object? DescribeWorkItem(Guid accountId) => accountId;
}
