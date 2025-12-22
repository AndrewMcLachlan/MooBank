using Asm.MooBank.Security;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Import(Guid InstrumentId, Guid AccountId, Stream Stream) : ICommand;

internal class ImportHandler(IImportTransactionsQueue importQueue, MooBank.Models.User user) : ICommandHandler<Import>
{
    public async ValueTask Handle(Import request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out Guid instrumentId, out Guid accountId, out Stream stream);

        // Read stream into byte array so it can be queued
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var fileData = memoryStream.ToArray();

        // Queue the import for background processing
        importQueue.QueueImport(instrumentId, accountId, user, fileData);
    }
}
