﻿namespace Asm.MooBank.Domain.Entities.ReferenceData;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(DateOnly date, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default);

    StockPriceHistory AddStockPrice(StockPriceHistory stockPrice);

    ExchangeRate AddExchangeRate(ExchangeRate exchangeRate);

    Task<IEnumerable<ExchangeRate>> GetExchangeRates();
}
