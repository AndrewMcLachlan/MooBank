namespace Asm.MooBank.Importers;

public interface IImporterFactory
{
    Task<IImporter?> Create(Guid accountId, CancellationToken cancellationToken = default);

    IImporter? Create(string? importerType);
}
