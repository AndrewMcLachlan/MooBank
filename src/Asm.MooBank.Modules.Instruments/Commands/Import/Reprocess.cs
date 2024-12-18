using Asm.MooBank.Importers;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Reprocess(Guid InstrumentId) : ICommand;

internal class ReprocessHandler(IUnitOfWork unitOfWork, IImporterFactory importerFactory) : ICommandHandler<Reprocess>
{
    public async ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        var importer = await importerFactory.Create(request.InstrumentId, cancellationToken) ?? throw new ArgumentException("Import is not supported", nameof(request));

        await importer.Reprocess(request.InstrumentId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
