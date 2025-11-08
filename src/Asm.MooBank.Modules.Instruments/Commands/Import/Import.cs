using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Import(Guid InstrumentId, Guid AccountId, Stream Stream) : ICommand;

internal class ImportHandler(IInstrumentRepository instrumentRepository, IRuleRepository ruleRepository, IImporterFactory importerFactory, IUnitOfWork unitOfWork) : ICommandHandler<Import>
{
    public async ValueTask Handle(Import request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out Guid instrumentId, out Guid accountId, out Stream stream);

        var instrument = await instrumentRepository.Get(instrumentId, cancellationToken) ?? throw new NotFoundException($"Instrument with ID {instrumentId} not found");

        IImporter importer = await importerFactory.Create(instrumentId, accountId, cancellationToken) ?? throw new ArgumentException("Import is not supported", nameof(request));

        var importResult = await importer.Import(instrumentId, accountId, stream, cancellationToken);

        await ApplyRules(instrument, importResult.Transactions, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyRules(Instrument instrument, IEnumerable<Domain.Entities.Transactions.Transaction> transactions, CancellationToken cancellationToken = default)
    {
        var rules = await ruleRepository.GetForInstrument(instrument.Id, cancellationToken);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.Tags).Distinct(new TagEqualityComparer()).ToList();

            transaction.AddOrUpdateSplit(applicableTags);
        }
    }
}
