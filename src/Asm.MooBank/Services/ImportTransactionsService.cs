using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public interface IImportTransactionsService
{
    Task Import(ImportWorkItem import, CancellationToken cancellationToken = default);
}

internal class ImportTransactionsService(IInstrumentRepository instrumentRepository, IRuleRepository ruleRepository, IImporterFactory importerFactory, IUnitOfWork unitOfWork, ILogger<ImportTransactionsService> logger) : IImportTransactionsService
{
    public async Task Import(ImportWorkItem workItem, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Import Transactions Service is starting.");

        try
        {
            logger.LogInformation("Processing import for instrument {InstrumentId}, account {AccountId}, user {UserId}", workItem.InstrumentId, workItem.AccountId, workItem.User.Id);

            var instrument = await instrumentRepository.Get(workItem.InstrumentId, cancellationToken)
                ?? throw new InvalidOperationException($"Instrument with ID {workItem.InstrumentId} not found");

            var importer = await importerFactory.Create(workItem.InstrumentId, workItem.AccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Import is not supported for account with ID: {workItem.AccountId}");

            using var stream = new MemoryStream(workItem.FileData);
            var importResult = await importer.Import(workItem.InstrumentId, workItem.AccountId, stream, cancellationToken);

            await ApplyRules(ruleRepository, instrument, importResult.Transactions, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Import completed for instrument {InstrumentId}, account {AccountId}. {Count} transactions imported.",
                workItem.InstrumentId, workItem.AccountId, importResult.Transactions.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred importing transactions for account {AccountId}.", workItem.AccountId);
        }
    }

    private static async Task ApplyRules(IRuleRepository ruleRepository, Domain.Entities.Instrument.Instrument instrument, IEnumerable<Domain.Entities.Transactions.Transaction> transactions, CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.GetForInstrument(instrument.Id, cancellationToken);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules
                .Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false)
                .SelectMany(r => r.Tags)
                .Distinct(new TagEqualityComparer())
                .ToList();

            transaction.AddOrUpdateSplit(applicableTags);
        }
    }
}
