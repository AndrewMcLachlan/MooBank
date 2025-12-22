using System.Collections.Concurrent;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public class ImportTransactionsService(IImportTransactionsQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
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
                    _logger.LogInformation("Processing import for instrument {InstrumentId}, account {AccountId}, user {UserId}", workItem.InstrumentId, workItem.AccountId, workItem.User.Id);

                    using var scope = _serviceScopeFactory.CreateScope();

                    // Set the user context for this scope
                    var userDataProvider = scope.ServiceProvider.GetRequiredService<ISettableUserDataProvider>();
                    userDataProvider.SetUser(workItem.User);

                    var instrumentRepository = scope.ServiceProvider.GetRequiredService<IInstrumentRepository>();
                    var ruleRepository = scope.ServiceProvider.GetRequiredService<IRuleRepository>();
                    var importerFactory = scope.ServiceProvider.GetRequiredService<IImporterFactory>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var instrument = await instrumentRepository.Get(workItem.InstrumentId, cancellationToken)
                        ?? throw new InvalidOperationException($"Instrument with ID {workItem.InstrumentId} not found");

                    var importer = await importerFactory.Create(workItem.InstrumentId, workItem.AccountId, cancellationToken)
                        ?? throw new InvalidOperationException($"Import is not supported for account with ID: {workItem.AccountId}");

                    using var stream = new MemoryStream(workItem.FileData);
                    var importResult = await importer.Import(workItem.InstrumentId, workItem.AccountId, stream, cancellationToken);

                    await ApplyRules(ruleRepository, instrument, importResult.Transactions, cancellationToken);

                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Import completed for instrument {InstrumentId}, account {AccountId}. {Count} transactions imported.",
                        workItem.InstrumentId, workItem.AccountId, importResult.Transactions.Count());
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

public record ImportWorkItem(Guid InstrumentId, Guid AccountId, User User, byte[] FileData);

public interface IImportTransactionsQueue
{
    void QueueImport(Guid instrumentId, Guid accountId, User user, byte[] fileData);

    Task<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

public class ImportTransactionsQueue : IImportTransactionsQueue, IDisposable
{
    private readonly ConcurrentQueue<ImportWorkItem> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueImport(Guid instrumentId, Guid accountId, User user, byte[] fileData)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue(new ImportWorkItem(instrumentId, accountId, user, fileData));
        _signal.Release();
    }

    public async Task<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem!;
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
