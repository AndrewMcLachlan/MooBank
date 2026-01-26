using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using QuarterEntity = Asm.MooBank.Domain.Entities.QuarterEntity;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="CpiService"/> service.
/// Tests cover CPI-adjusted value calculations across different time periods.
/// </summary>
public class CpiServiceTests
{
    #region CalculateAdjustedValue

    /// <summary>
    /// Given no CPI changes exist after the start date
    /// When CalculateAdjustedValue is called
    /// Then the original value should be returned unchanged
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_NoCpiChangesAfterStartDate_ReturnsOriginalValue()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2023, 1, 1.5m),
            CreateCpiChange(2023, 2, 1.2m),
        };
        var service = CreateService(cpiChanges);

        // Act - Start date is after all CPI changes
        var result = await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 1, 1), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1000m, result);
    }

    /// <summary>
    /// Given one CPI change of 2% after the start date
    /// When CalculateAdjustedValue is called with $1000
    /// Then $1020 should be returned (1000 * 1.02)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_OneCpiChangeAfterStartDate_AppliesChange()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, 2.0m), // 2% increase in Q2 2024
        };
        var service = CreateService(cpiChanges);

        // Act - Start date is Q1 2024, so Q2 change applies
        var result = await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1020m, result);
    }

    /// <summary>
    /// Given multiple CPI changes after the start date
    /// When CalculateAdjustedValue is called
    /// Then all changes should be applied cumulatively
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_MultipleCpiChanges_AppliesCumulatively()
    {
        // Arrange - Two quarters of 2% each
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, 2.0m), // Q2: 2%
            CreateCpiChange(2024, 3, 2.0m), // Q3: 2%
        };
        var service = CreateService(cpiChanges);

        // Act - Start date is Q1 2024
        var result = await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);

        // Assert - 1000 * 1.02 * 1.02 = 1040.4
        Assert.Equal(1040.4m, result);
    }

    /// <summary>
    /// Given CPI changes before and after the start date
    /// When CalculateAdjustedValue is called
    /// Then only changes after the start date should be applied
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_MixedCpiChanges_AppliesOnlyAfterStartDate()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2023, 4, 5.0m), // Before start - should NOT apply
            CreateCpiChange(2024, 1, 3.0m), // Same quarter as start - should NOT apply
            CreateCpiChange(2024, 2, 2.0m), // After start - should apply
        };
        var service = CreateService(cpiChanges);

        // Act - Start date is in Q1 2024
        var result = await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 2, 15), TestContext.Current.CancellationToken);

        // Assert - Only Q2 2024 change applies: 1000 * 1.02 = 1020
        Assert.Equal(1020m, result);
    }

    /// <summary>
    /// Given a negative CPI change (deflation)
    /// When CalculateAdjustedValue is called
    /// Then the value should decrease
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_NegativeCpiChange_DecreasesValue()
    {
        // Arrange - 1% deflation
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, -1.0m),
        };
        var service = CreateService(cpiChanges);

        // Act
        var result = await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);

        // Assert - 1000 * 0.99 = 990
        Assert.Equal(990m, result);
    }

    #endregion

    #region Overloads

    /// <summary>
    /// Given a DateTime input
    /// When CalculateAdjustedValue is called
    /// Then it should produce the same result as DateOnly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_WithDateTime_ProducesSameResult()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, 2.0m),
        };
        var service = CreateService(cpiChanges);
        var dateTime = new DateTime(2024, 1, 15);
        var dateOnly = new DateOnly(2024, 1, 15);

        // Act
        var resultDateTime = await service.CalculateAdjustedValue(1000m, dateTime, TestContext.Current.CancellationToken);
        var resultDateOnly = await service.CalculateAdjustedValue(1000m, dateOnly, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(resultDateOnly, resultDateTime);
    }

    /// <summary>
    /// Given a DateTimeOffset input
    /// When CalculateAdjustedValue is called
    /// Then it should produce the same result as DateOnly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_WithDateTimeOffset_ProducesSameResult()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, 2.0m),
        };
        var service = CreateService(cpiChanges);
        var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var dateOnly = new DateOnly(2024, 1, 15);

        // Act
        var resultDateTimeOffset = await service.CalculateAdjustedValue(1000m, dateTimeOffset, TestContext.Current.CancellationToken);
        var resultDateOnly = await service.CalculateAdjustedValue(1000m, dateOnly, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(resultDateOnly, resultDateTimeOffset);
    }

    #endregion

    #region Caching

    /// <summary>
    /// Given the repository is called once
    /// When CalculateAdjustedValue is called multiple times
    /// Then the repository should only be called once due to caching
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CalculateAdjustedValue_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var cpiChanges = new List<CpiChange>
        {
            CreateCpiChange(2024, 2, 2.0m),
        };
        var repositoryMock = new Mock<IReferenceDataRepository>();
        repositoryMock.Setup(r => r.GetCpiChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cpiChanges);

        var service = CreateService(repositoryMock.Object);

        // Act - Call multiple times
        await service.CalculateAdjustedValue(1000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);
        await service.CalculateAdjustedValue(2000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);
        await service.CalculateAdjustedValue(3000m, new DateOnly(2024, 1, 15), TestContext.Current.CancellationToken);

        // Assert - Repository should only be called once
        repositoryMock.Verify(r => r.GetCpiChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    private static CpiChange CreateCpiChange(int year, int quarter, decimal changePercent) =>
        new()
        {
            Quarter = new QuarterEntity(year, quarter),
            ChangePercent = changePercent,
        };

    private static CpiService CreateService(List<CpiChange> cpiChanges)
    {
        var repositoryMock = new Mock<IReferenceDataRepository>();
        repositoryMock.Setup(r => r.GetCpiChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cpiChanges);

        return CreateService(repositoryMock.Object);
    }

    private static CpiService CreateService(IReferenceDataRepository repository)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheProvider = new MemoryCacheProvider(memoryCache);
        var appCache = new CachingService(new Lazy<ICacheProvider>(() => cacheProvider));

        return new CpiService(repository, appCache);
    }
}
