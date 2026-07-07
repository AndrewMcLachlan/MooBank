using Asm.Domain;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using Asm.Security;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Accounts.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        AuditingUnitOfWorkMock = new Mock<IAuditingUnitOfWork>();
        AuditingUnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertGroupPermission(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        LogicalAccountRepositoryMock = new Mock<ILogicalAccountRepository>();

        InstrumentRepositoryMock = new Mock<IInstrumentRepository>();

        TagRepositoryMock = new Mock<ITagRepository>();

        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CurrencyConverterMock.Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns<decimal, string>((amount, _) => amount);

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IAuditingUnitOfWork> AuditingUnitOfWorkMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<ILogicalAccountRepository> LogicalAccountRepositoryMock { get; }

    public Mock<IInstrumentRepository> InstrumentRepositoryMock { get; }

    public Mock<ITagRepository> TagRepositoryMock { get; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; }

    public User User { get; private set; }

    public void SecurityFailGroupPermission()
    {
        SecurityMock.Setup(s => s.AssertGroupPermission(It.IsAny<Guid>()))
            .Throws(new NotAuthorisedException());
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public static User CreateTestUser(
        Guid? id = null,
        string email = "test@example.com",
        string currency = "AUD",
        Guid? familyId = null,
        Guid? primaryAccountId = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            EmailAddress = email,
            FirstName = "Test",
            LastName = "User",
            Currency = currency,
            FamilyId = familyId ?? Guid.NewGuid(),
            PrimaryAccountId = primaryAccountId,
            Accounts = [],
            SharedAccounts = [],
            Groups = [],
        };
    }
}
