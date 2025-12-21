using System.Collections.Concurrent;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
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

                var rules = await transactionTagRuleRepository.GetForInstrument(accountId, cancellationToken);

                // Parallel: compute rule matches (read-only, thread-safe)
                var ruleMatches = transactions.AsParallel().Select(transaction =>
                {
                    var applicableRules = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                    var tags = applicableRules.SelectMany(r => r.Tags).Distinct().ToList();
                    var notes = String.Join(". ", applicableRules.Where(r => !String.IsNullOrWhiteSpace(r.Description)).Select(r => r.Description));
                    return (transaction, tags, notes);
                }).ToList();

                // Sequential: apply mutations to tracked entities
                foreach (var (transaction, tags, notes) in ruleMatches)
                {
                    transaction.AddOrUpdateSplit(tags);
                    if (String.IsNullOrEmpty(transaction.Notes))
                    {
                        transaction.Notes = notes;
                    }
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred running rules for account {AccountId}.", accountId);
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

public class RunRulesQueue : IRunRulesQueue, IDisposable
{
    private readonly ConcurrentQueue<Guid> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueRunRules(Guid accountId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue(accountId);
        _signal.Release();
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _signal.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
