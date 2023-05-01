using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.Repositories;

public class ReferenceDataRepository : IReferenceDataRepository
{
    private MooBankContext DataContext { get; }

    public ReferenceDataRepository(MooBankContext dataContext)
    {
        DataContext = dataContext;
    }

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) =>
        await DataContext.ImporterTypes.ToListAsync(cancellationToken);
}
