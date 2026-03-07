using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Microsoft.Extensions.Logging;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;

namespace Asm.MooBank.Services;

public interface IRunRulesService
{
    Task RunRules(Guid accountId, CancellationToken cancellationToken = default);
}

internal class RunRulesService(ITransactionRepository transactionRepository, IRuleRepository transactionTagRuleRepository, IUnitOfWork unitOfWork, ILogger<RunRulesService> logger) : IRunRulesService
{
    public async Task RunRules(Guid accountId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Run Rules Service is processing account {AccountId}.", accountId);

        try
        {
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

            logger.LogInformation("Run Rules completed for account {AccountId}. {Count} transactions processed.", accountId, transactions.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred running rules for account {AccountId}.", accountId);
            throw;
        }
    }
}
