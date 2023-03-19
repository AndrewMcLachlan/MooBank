using Asm.MooBank.Domain.Entities;

namespace Asm.MooBank.Domain.Repositories;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);
}
