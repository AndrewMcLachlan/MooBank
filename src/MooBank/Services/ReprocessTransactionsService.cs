using Asm.Domain;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public interface IReprocessTransactionsService
{
    Task Reprocess(ReprocessWorkItem workItem, CancellationToken cancellationToken = default);
}

internal class ReprocessTransactionsService(IImporterFactory importerFactory, IUnitOfWork unitOfWork, ILogger<ReprocessTransactionsService> logger) : IReprocessTransactionsService
{
    public async Task Reprocess(ReprocessWorkItem workItem, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reprocess Transactions Service is processing instrument {InstrumentId}, account {AccountId}.",
            workItem.InstrumentId, workItem.AccountId);

        try
        {
            var importer = await importerFactory.Create(workItem.InstrumentId, workItem.AccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Import is not supported for account with ID: {workItem.AccountId}");

            await importer.Reprocess(workItem.InstrumentId, workItem.AccountId, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Reprocess completed for instrument {InstrumentId}, account {AccountId}.",
                workItem.InstrumentId, workItem.AccountId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred reprocessing transactions for account {AccountId}.", workItem.AccountId);
            throw;
        }
    }
}
