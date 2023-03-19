﻿using System.Collections.Concurrent;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public class RunRulesService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRunRulesQueue _taskQueue;

    public RunRulesService(IRunRulesQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory)
    {
        _taskQueue = taskQueue;
        _logger = loggerFactory.CreateLogger<RunRulesService>();
        _serviceScopeFactory = serviceScopeFactory;
    }

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
                var transactionTagRuleRepository = scope.ServiceProvider.GetRequiredService<ITransactionTagRuleRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var transactions = await transactionRepository.GetTransactions(accountId, cancellationToken);

                var rules = await transactionTagRuleRepository.GetForAccount(accountId, cancellationToken);

                var updatedTransactions = new List<Transaction>();

                foreach (var transaction in transactions)
                {
                    var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.TransactionTags.Select(t => new Domain.Entities.TransactionTag { TransactionTagId = t.TransactionTagId })).Distinct();

                    transaction.TransactionTags.AddRange(applicableTags);
                    //updatedTransactions.Add(await transactionRepository.AddTransactionTags(transaction.Id, applicableTags));
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
