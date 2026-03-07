#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Stocks.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        StockHoldingRepositoryMock = new Mock<IStockHoldingRepository>();
        SecurityMock = new Mock<ISecurity>();
        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CpiServiceMock = new Mock<ICpiService>();

        // Default currency converter behavior - pass through
        CurrencyConverterMock
            .Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns((decimal amount, string _) => amount);

        // Default CPI service behavior - return same value
        CpiServiceMock
            .Setup(c => c.CalculateAdjustedValue(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal value, DateTimeOffset _, CancellationToken _) => value);
        CpiServiceMock
            .Setup(c => c.CalculateAdjustedValue(It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal value, DateTime _, CancellationToken _) => value);

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IStockHoldingRepository> StockHoldingRepositoryMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; }

    public Mock<ICpiService> CpiServiceMock { get; }

    public User User { get; private set; }

    public void SetUser(User user)
    {
        User = user;
    }

    public static User CreateTestUser(
        Guid? id = null,
        string email = "test@example.com",
        string currency = "AUD",
        Guid? familyId = null,
        IEnumerable<Guid>? accounts = null,
        IEnumerable<Guid>? sharedAccounts = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            EmailAddress = email,
            FirstName = "Test",
            LastName = "User",
            Currency = currency,
            FamilyId = familyId ?? Guid.NewGuid(),
            PrimaryAccountId = null,
            Accounts = accounts ?? [],
            SharedAccounts = sharedAccounts ?? [],
            Groups = [],
        };
    }
}
