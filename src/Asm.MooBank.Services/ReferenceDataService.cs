using IReferenceDataRepository = Asm.MooBank.Domain.Entities.ReferenceData.IReferenceDataRepository;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services;

public interface IReferenceDataService
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);
}


internal class ReferenceDataService : IReferenceDataService
{

    private readonly IReferenceDataRepository _referenceDataRepository;

    public ReferenceDataService(IReferenceDataRepository referenceDataRepository)
    {
        _referenceDataRepository = referenceDataRepository;
    }

    public Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => _referenceDataRepository.GetImporterTypes(cancellationToken).ToModelAsync(cancellationToken);
}
