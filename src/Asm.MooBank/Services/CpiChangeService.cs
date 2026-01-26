using Asm.Domain;
using Asm.MooBank.Abs;
using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Services;

public interface ICpiChangeService
{
    Task UpdateWithCpiChanges(CancellationToken cancellationToken = default);
}

internal class CpiChangeService(IUnitOfWork unitOfWork, IAbsClient absClient, IReferenceDataRepository referenceDataRepository) : ICpiChangeService
{

    public async Task UpdateWithCpiChanges(CancellationToken cancellationToken = default)
    {
        var existingChanges = await referenceDataRepository.GetCpiChanges(cancellationToken);

        var changes = await absClient.GetCpiChanges(DateOnly.Today.AddMonths(-6), null, cancellationToken);

        foreach (var change in changes)
        {
            var existingChange = existingChanges.SingleOrDefault(c => c.Quarter == change.Quarter);
            if (existingChange == null)
            {
                referenceDataRepository.AddCpiChange(new()
                {
                    Quarter = change.Quarter,
                    ChangePercent = change.ChangePercent,
                });
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

}
