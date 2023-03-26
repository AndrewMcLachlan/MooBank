using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.Repositories;

public class ReferenceDataRepository : IReferenceDataRepository
{
    private BankPlusContext DataContext { get; }

    public ReferenceDataRepository(BankPlusContext dataContext)
    {
        DataContext = dataContext;
    }

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) =>
        await DataContext.ImporterTypes.ToListAsync(cancellationToken);
}
