#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Modules.Forecast.Services;
using Asm.MooBank.Security;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Forecast.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        ForecastRepositoryMock = new Mock<IForecastRepository>();
        SecurityMock = new Mock<ISecurity>();
        ForecastEngineMock = new Mock<IForecastEngine>();
        ReportRepositoryMock = new Mock<IReportRepository>();
        InstrumentRepositoryMock = new Mock<IInstrumentRepository>();

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IForecastRepository> ForecastRepositoryMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<IForecastEngine> ForecastEngineMock { get; }

    public Mock<IReportRepository> ReportRepositoryMock { get; }

    public Mock<IInstrumentRepository> InstrumentRepositoryMock { get; }

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
