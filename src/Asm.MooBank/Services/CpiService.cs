﻿using Asm.Domain;
using Asm.MooBank.Abs;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using LazyCache;

namespace Asm.MooBank.Services;

public interface ICpiService
{
    Task<decimal> CalculateAdjustedValue(decimal value, DateTimeOffset startDate, CancellationToken cancellationToken = default);

    Task<decimal> CalculateAdjustedValue(decimal value, DateTime startDate, CancellationToken cancellationToken = default);

    Task<decimal> CalculateAdjustedValue(decimal value, DateOnly startDate, CancellationToken cancellationToken = default);
}

internal class CpiService(IReferenceDataRepository referenceDataRepository, IAppCache cache) : ICpiService
{
    public Task<decimal> CalculateAdjustedValue(decimal value, DateTimeOffset startDate, CancellationToken cancellationToken = default) =>
        CalculateAdjustedValue(value, DateOnly.FromDateTime(startDate.Date), cancellationToken);

    public Task<decimal> CalculateAdjustedValue(decimal value, DateTime startDate, CancellationToken cancellationToken = default) =>
        CalculateAdjustedValue(value, DateOnly.FromDateTime(startDate.Date), cancellationToken);

    public async Task<decimal> CalculateAdjustedValue(decimal value, DateOnly startDate, CancellationToken cancellationToken = default)
    {
        var changes = await cache.GetOrAdd("CpiChanges", async () => await referenceDataRepository.GetCpiChanges(cancellationToken), TimeSpan.FromDays(1));

        var startQuarter = Quarter.FromDate(startDate);
        decimal adjustedValue = value;
        foreach (var change in changes.Where(c => (Quarter)c.Quarter > startQuarter).OrderBy(c => c.Quarter))
        {
            adjustedValue *= 1 + (change.ChangePercent / 100);
        }
        return adjustedValue;
    }
}
