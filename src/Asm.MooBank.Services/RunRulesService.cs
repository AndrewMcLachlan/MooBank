using System.Collections.Concurrent;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;

namespace Asm.MooBank.Services;

public class RunRulesService(IRunRulesQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RunRulesService>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IRunRulesQueue _taskQueue = taskQueue;

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var accountId = await _taskQueue.DequeueAsync(cancellationToken);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var transactionTagRuleRepository = scope.ServiceProvider.GetRequiredService<IRuleRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var transactions = await transactionRepository.GetTransactions(accountId, cancellationToken);

                var rules = await transactionTagRuleRepository.GetForAccount(accountId, cancellationToken);

                var updatedTransactions = new List<Transaction>();

                foreach (var transaction in transactions)
                {
                    var applicableRules = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false);
                    var applicableTags = applicableRules.SelectMany(r => r.Tags).Distinct();

                    transaction.AddOrUpdateSplit(applicableTags);
                    if (String.IsNullOrEmpty(transaction.Notes))
                    {
                        transaction.Notes = String.Join(". ", applicableRules.Select(r => r.Description));
                    }
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred running rules for account {AccountId}.", nameof(accountId));
            }
        }

        _logger.LogInformation("Queued Hosted Service is stopping.");
    }
}

public interface IRunRulesQueue
{
    void QueueRunRules(Guid accountId);

    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}

public class RunRulesQueue : IRunRulesQueue
{
    private readonly ConcurrentQueue<Guid> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void QueueRunRules(Guid accountId)
    {
        _workItems.Enqueue(accountId);
        _signal.Release();
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }
}
