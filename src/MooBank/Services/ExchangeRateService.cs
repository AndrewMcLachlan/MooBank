using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.ExchangeRateApi;

namespace Asm.MooBank.Services;

public interface IExchangeRateService
{
    Task UpdateExchangeRates(CancellationToken cancellationToken = default);
}

public class ExchangeRateService(IUnitOfWork unitOfWork, IExchangeRateClient exchangeRateClient, IQueryable<Instrument> accounts, IQueryable<User> accountHolders, IReferenceDataRepository referenceDataRepository) : IExchangeRateService
{
    public async Task UpdateExchangeRates(CancellationToken cancellationToken = default)
    {
        // Materialise the currency sets up front, otherwise each Contains call below issues a query.
        var froms = accounts.Select(a => a.Currency).Distinct().ToList();
        var tos = accountHolders.Select(ah => ah.Currency).Distinct().ToHashSet();

        var existingRates = await referenceDataRepository.GetExchangeRates(cancellationToken);

        foreach (var from in froms)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rates = await exchangeRateClient.GetExchangeRates(from, cancellationToken);

            var toRates = rates.Where(tr => tos.Contains(tr.Key) && tr.Key != from);

            foreach (var to in toRates)
            {
                var existingRate = existingRates.SingleOrDefault(r => r.From == from && r.To == to.Key);

                if (existingRate != null)
                {
                    existingRate.Rate = to.Value;
                    existingRate.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    referenceDataRepository.AddExchangeRate(new()
                    {
                        From = from,
                        To = to.Key,
                        Rate = to.Value,
                        LastUpdated = DateTime.UtcNow,
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
