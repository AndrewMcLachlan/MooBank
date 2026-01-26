using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.ExchangeRateApi;
using Asm.MooBank.Services;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="ExchangeRateService"/> service.
/// Tests cover exchange rate updates from external API.
/// </summary>
public class ExchangeRateServiceTests
{
    #region UpdateExchangeRates

    /// <summary>
    /// Given no accounts or users
    /// When UpdateExchangeRates is called
    /// Then no rates should be fetched or added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_NoAccountsOrUsers_DoesNothing()
    {
        // Arrange
        var accounts = Array.Empty<Instrument>().AsQueryable();
        var users = Array.Empty<User>().AsQueryable();

        var (service, repositoryMock, exchangeRateClientMock) = CreateService(accounts, users, [], []);

        // Act
        await service.UpdateExchangeRates();

        // Assert
        exchangeRateClientMock.Verify(c => c.GetExchangeRates(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(r => r.AddExchangeRate(It.IsAny<ExchangeRate>()), Times.Never);
    }

    /// <summary>
    /// Given accounts in AUD and users in AUD (same currency)
    /// When UpdateExchangeRates is called
    /// Then no new rates should be added (same currency doesn't need conversion)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_SameCurrency_AddsNoRates()
    {
        // Arrange
        var accounts = CreateAccounts("AUD").AsQueryable();
        var users = CreateUsers("AUD").AsQueryable();
        var apiRates = new Dictionary<string, decimal> { { "AUD", 1.0m } };

        var (service, repositoryMock, _) = CreateService(accounts, users, [], apiRates);

        // Act
        await service.UpdateExchangeRates();

        // Assert - AUD to AUD shouldn't create a rate
        repositoryMock.Verify(r => r.AddExchangeRate(It.IsAny<ExchangeRate>()), Times.Never);
    }

    /// <summary>
    /// Given accounts in AUD and users in USD, no existing rates
    /// When UpdateExchangeRates is called
    /// Then a new AUD->USD rate should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_NewRate_AddsRate()
    {
        // Arrange
        var accounts = CreateAccounts("AUD").AsQueryable();
        var users = CreateUsers("USD").AsQueryable();
        var apiRates = new Dictionary<string, decimal> { { "USD", 0.65m }, { "AUD", 1.0m } };

        var (service, repositoryMock, _) = CreateService(accounts, users, [], apiRates);

        // Act
        await service.UpdateExchangeRates();

        // Assert
        repositoryMock.Verify(r => r.AddExchangeRate(It.Is<ExchangeRate>(
            e => e.From == "AUD" && e.To == "USD" && e.Rate == 0.65m)), Times.Once);
    }

    /// <summary>
    /// Given an existing rate that needs updating
    /// When UpdateExchangeRates is called
    /// Then the existing rate should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_ExistingRate_UpdatesRate()
    {
        // Arrange
        var accounts = CreateAccounts("AUD").AsQueryable();
        var users = CreateUsers("USD").AsQueryable();
        var existingRate = new ExchangeRate { From = "AUD", To = "USD", Rate = 0.60m };
        var apiRates = new Dictionary<string, decimal> { { "USD", 0.65m }, { "AUD", 1.0m } };

        var (service, repositoryMock, _) = CreateService(accounts, users, [existingRate], apiRates);

        // Act
        await service.UpdateExchangeRates();

        // Assert - Rate should be updated, not added
        Assert.Equal(0.65m, existingRate.Rate);
        repositoryMock.Verify(r => r.AddExchangeRate(It.IsAny<ExchangeRate>()), Times.Never);
    }

    /// <summary>
    /// Given multiple accounts with different currencies
    /// When UpdateExchangeRates is called
    /// Then rates should be fetched for each unique account currency
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_MultipleAccountCurrencies_FetchesAllRates()
    {
        // Arrange
        var accounts = CreateAccounts("AUD", "GBP", "EUR").AsQueryable();
        var users = CreateUsers("USD").AsQueryable();
        var apiRates = new Dictionary<string, decimal> { { "USD", 0.65m } };

        var (service, _, exchangeRateClientMock) = CreateService(accounts, users, [], apiRates);

        // Act
        await service.UpdateExchangeRates();

        // Assert - Should fetch rates for AUD, GBP, and EUR
        exchangeRateClientMock.Verify(c => c.GetExchangeRates("AUD", It.IsAny<CancellationToken>()), Times.Once);
        exchangeRateClientMock.Verify(c => c.GetExchangeRates("GBP", It.IsAny<CancellationToken>()), Times.Once);
        exchangeRateClientMock.Verify(c => c.GetExchangeRates("EUR", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given rates are updated
    /// When UpdateExchangeRates completes
    /// Then SaveChangesAsync should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateExchangeRates_Always_SavesChanges()
    {
        // Arrange
        var accounts = Array.Empty<Instrument>().AsQueryable();
        var users = Array.Empty<User>().AsQueryable();

        var (service, _, _, unitOfWorkMock) = CreateServiceWithUnitOfWork(accounts, users, [], []);

        // Act
        await service.UpdateExchangeRates();

        // Assert
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    private static Instrument[] CreateAccounts(params string[] currencies) =>
        currencies.Select(c => new TestInstrument { Name = "Test", Currency = c }).ToArray();

    private static User[] CreateUsers(params string[] currencies) =>
        currencies.Select(c => new User { EmailAddress = "test@test.com", Currency = c, FamilyId = Guid.NewGuid() }).ToArray();

    private static (IExchangeRateService service, Mock<IReferenceDataRepository> repositoryMock, Mock<IExchangeRateClient> exchangeRateClientMock) CreateService(
        IQueryable<Instrument> accounts,
        IQueryable<User> users,
        List<ExchangeRate> existingRates,
        Dictionary<string, decimal> apiRates)
    {
        var (service, repositoryMock, exchangeRateClientMock, _) = CreateServiceWithUnitOfWork(accounts, users, existingRates, apiRates);
        return (service, repositoryMock, exchangeRateClientMock);
    }

    private static (IExchangeRateService service, Mock<IReferenceDataRepository> repositoryMock, Mock<IExchangeRateClient> exchangeRateClientMock, Mock<IUnitOfWork> unitOfWorkMock) CreateServiceWithUnitOfWork(
        IQueryable<Instrument> accounts,
        IQueryable<User> users,
        List<ExchangeRate> existingRates,
        Dictionary<string, decimal> apiRates)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var exchangeRateClientMock = new Mock<IExchangeRateClient>();
        var repositoryMock = new Mock<IReferenceDataRepository>();

        repositoryMock.Setup(r => r.GetExchangeRates())
            .ReturnsAsync(existingRates);

        exchangeRateClientMock.Setup(c => c.GetExchangeRates(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiRates);

        var service = new ExchangeRateService(
            unitOfWorkMock.Object,
            exchangeRateClientMock.Object,
            accounts,
            users,
            repositoryMock.Object);

        return (service, repositoryMock, exchangeRateClientMock, unitOfWorkMock);
    }
}

// Test instrument for mocking
file class TestInstrument : Instrument
{
    public TestInstrument() : base(Guid.NewGuid())
    {
        Name = "Test Instrument";
    }
}
