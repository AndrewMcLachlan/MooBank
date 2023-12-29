using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.ExchangeRateApi;

namespace Asm.MooBank.Services;

public interface IExchangeRateService
{
    Task UpdateExchangeRates();
}

internal class ExchangeRateService(IUnitOfWork unitOfWork, IExchangeRateClient exchangeRateClient, IQueryable<Account> accounts, IQueryable<AccountHolder> accountHolders, IReferenceDataRepository referenceDataRepository) : IExchangeRateService
{
    public async Task UpdateExchangeRates()
    {
        var froms = accounts.Select(a => a.Currency).Distinct();
        var tos = accountHolders.Select(ah => ah.Currency).Distinct();

        var existingRates = await referenceDataRepository.GetExchangeRates();

        foreach (var from in froms)
        {
            var rates = await exchangeRateClient.GetExchangeRates(from);

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

        await unitOfWork.SaveChangesAsync();
    }
}
