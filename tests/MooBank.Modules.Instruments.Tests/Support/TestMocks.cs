#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Queues;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Instruments.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        InstrumentRepositoryMock = new Mock<IInstrumentRepository>();
        RuleRepositoryMock = new Mock<IRuleRepository>();
        TagRepositoryMock = new Mock<ITagRepository>();
        SecurityMock = new Mock<ISecurity>();
        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        ImportQueueMock = new Mock<IImportTransactionsQueue>();
        ReprocessQueueMock = new Mock<IReprocessTransactionsQueue>();
        RunRulesQueueMock = new Mock<IRunRulesQueue>();

        // Default currency converter behavior - returns same amount (no conversion)
        CurrencyConverterMock
            .Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns((decimal amount, string currency) => amount);

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IInstrumentRepository> InstrumentRepositoryMock { get; }

    public Mock<IRuleRepository> RuleRepositoryMock { get; }

    public Mock<ITagRepository> TagRepositoryMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; }

    public Mock<IImportTransactionsQueue> ImportQueueMock { get; }

    public Mock<IReprocessTransactionsQueue> ReprocessQueueMock { get; }

    public Mock<IRunRulesQueue> RunRulesQueueMock { get; }

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
