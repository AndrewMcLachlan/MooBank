using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="CurrencyConverter"/> service.
/// Tests cover same currency, forward/reverse exchange rate lookups, and missing rate handling.
/// </summary>
public class CurrencyConverterTests
{
    /// <summary>
    /// Given user currency is AUD
    /// When CurrencyConverter.Convert is called with AUD amount
    /// Then the same amount should be returned (1:1 conversion)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_SameCurrency_ReturnsSameAmount()
    {
        // Arrange
        var converter = CreateConverter("AUD", []);

        // Act
        var result = converter.Convert(100m, "AUD");

        // Assert
        Assert.Equal(100m, result);
    }

    /// <summary>
    /// Given user currency is USD and exchange rate AUD->USD = 0.65
    /// When CurrencyConverter.Convert is called with 100 AUD
    /// Then 65 should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_WithForwardExchangeRate_AppliesRate()
    {
        // Arrange
        var exchangeRates = new List<ExchangeRate>
        {
            new() { From = "AUD", To = "USD", Rate = 0.65m },
        };
        var converter = CreateConverter("USD", exchangeRates);

        // Act
        var result = converter.Convert(100m, "AUD");

        // Assert
        Assert.Equal(65m, result);
    }

    /// <summary>
    /// Given user currency is AUD and exchange rate GBP->AUD = 1.90
    /// When CurrencyConverter.Convert is called with 100 GBP
    /// Then 190 should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_WithReverseExchangeRate_AppliesRate()
    {
        // Arrange
        var exchangeRates = new List<ExchangeRate>
        {
            new() { From = "GBP", To = "AUD", Rate = 1.90m },
        };
        var converter = CreateConverter("AUD", exchangeRates);

        // Act
        var result = converter.Convert(100m, "GBP");

        // Assert
        Assert.Equal(190m, result);
    }

    /// <summary>
    /// Given user currency is JPY and no exchange rate exists for AUD->JPY
    /// When CurrencyConverter.Convert is called with 100 AUD
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_WithMissingExchangeRate_ReturnsNull()
    {
        // Arrange
        var converter = CreateConverter("JPY", []);

        // Act
        var result = converter.Convert(100m, "AUD");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Given an extreme exchange rate (very high)
    /// When CurrencyConverter.Convert is called
    /// Then the conversion should handle large numbers correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_ExtremeHighRate_HandlesCorrectly()
    {
        // Arrange - Very high rate like JPY
        var exchangeRates = new List<ExchangeRate>
        {
            new() { From = "USD", To = "JPY", Rate = 150.5m },
        };
        var converter = CreateConverter("JPY", exchangeRates);

        // Act
        var result = converter.Convert(1000m, "USD");

        // Assert
        Assert.Equal(150500m, result);
    }

    /// <summary>
    /// Given an extreme exchange rate (very low)
    /// When CurrencyConverter.Convert is called
    /// Then the conversion should maintain precision
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_ExtremeLowRate_MaintainsPrecision()
    {
        // Arrange - Very low rate
        var exchangeRates = new List<ExchangeRate>
        {
            new() { From = "BTC", To = "USD", Rate = 0.000025m },
        };
        var converter = CreateConverter("USD", exchangeRates);

        // Act
        var result = converter.Convert(40000m, "BTC");

        // Assert
        Assert.Equal(1m, result);
    }

    /// <summary>
    /// Given a reverse rate lookup is required
    /// When CurrencyConverter.Convert is called
    /// Then it should use the reverse rate correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Convert_WithReverseRateLookup_UsesReverseRate()
    {
        // Arrange - Only have USD->AUD rate, but need AUD->USD
        var exchangeRates = new List<ExchangeRate>
        {
            new() { From = "USD", To = "AUD", Rate = 1.55m, ReverseRate = 0.645m },
        };
        var converter = CreateConverter("USD", exchangeRates);

        // Act - Converting AUD to USD should use reverse rate
        var result = converter.Convert(100m, "AUD");

        // Assert
        Assert.Equal(64.5m, result);
    }

    private static CurrencyConverter CreateConverter(string userCurrency, List<ExchangeRate> exchangeRates)
    {
        var referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
        referenceDataRepositoryMock.Setup(r => r.GetExchangeRates(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exchangeRates);

        // Create HybridCache using DI
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddHybridCache();
        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetRequiredService<HybridCache>();

        var user = new User
        {
            Id = TestModels.UserId,
            Currency = userCurrency,
            EmailAddress = "test@test.com",
            FamilyId = TestModels.FamilyId,
        };

        return new CurrencyConverter(referenceDataRepositoryMock.Object, user, cache);
    }
}
