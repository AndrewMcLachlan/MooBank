#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Modules.Forecast.Services;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Forecast.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        ForecastRepositoryMock = new Mock<IForecastRepository>();
        ForecastEngineMock = new Mock<IForecastEngine>();
        ReportReaderMock = new Mock<IReportReader>();
        ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>());
        InstrumentRepositoryMock = new Mock<IInstrumentRepository>();

        PlannedItemMatcherMock = new Mock<IPlannedItemMatcher>();
        PlannedItemMatcherMock
            .Setup(m => m.GetTaggedSpend(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IForecastRepository> ForecastRepositoryMock { get; }

    public Mock<IForecastEngine> ForecastEngineMock { get; }

    public Mock<IReportReader> ReportReaderMock { get; }

    public Mock<IInstrumentRepository> InstrumentRepositoryMock { get; }

    internal Mock<IPlannedItemMatcher> PlannedItemMatcherMock { get; }

    /// <summary>
    /// Sets the actual tagged spending the matcher will report.
    /// </summary>
    internal void SetTaggedSpend(params TaggedSpend[] spend) =>
        PlannedItemMatcherMock
            .Setup(m => m.GetTaggedSpend(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(spend);

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
