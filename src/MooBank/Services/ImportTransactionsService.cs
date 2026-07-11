using Asm.Domain;
using Asm.MooBank.Audit;
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

internal class ImportTransactionsService(IInstrumentRepository instrumentRepository, IImporterFactory importerFactory, IUnitOfWork unitOfWork, IAuditLogger audit, ILogger<ImportTransactionsService> logger) : IImportTransactionsService
{
    public async Task Import(ImportWorkItem workItem, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Import Transactions Service is starting.");

        try
        {
            audit.ImportStarted(workItem.User, workItem.InstrumentId, workItem.AccountId);

            var instrument = await instrumentRepository.Get(workItem.InstrumentId, cancellationToken)
                ?? throw new InvalidOperationException($"Instrument with ID {workItem.InstrumentId} not found");

            var importer = await importerFactory.Create(workItem.InstrumentId, workItem.AccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Import is not supported for account with ID: {workItem.AccountId}");

            using var stream = new MemoryStream(workItem.FileData);
            var importResult = await importer.Import(workItem.InstrumentId, workItem.AccountId, stream, cancellationToken);
            var transactions = importResult.Transactions as IReadOnlyCollection<Domain.Entities.Transactions.Transaction> ?? importResult.Transactions.ToList();

            ApplyRules(instrument, transactions);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            audit.ImportCompleted(workItem.User, workItem.InstrumentId, workItem.AccountId, transactions.Count);
        }
        catch (Exception ex)
        {
            // Log the rich context here, then rethrow so the failure isn't silently swallowed —
            // the background queue loop catches it to stay alive (matching RunRulesService).
            logger.LogError(ex, "Error occurred importing transactions for instrument {InstrumentId}, account {AccountId}. File size: {FileSize} bytes.", workItem.InstrumentId, workItem.AccountId, workItem.FileData.Length);
            throw;
        }
    }

    private static void ApplyRules(Domain.Entities.Instrument.Instrument instrument, IReadOnlyCollection<Domain.Entities.Transactions.Transaction> transactions)
    {
        var rules = instrument.Rules.OrderBy(r => r.Contains).ToList();

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
