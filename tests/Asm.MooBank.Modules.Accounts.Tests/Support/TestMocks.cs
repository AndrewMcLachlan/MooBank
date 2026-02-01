using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using Asm.Security;

namespace Asm.MooBank.Modules.Accounts.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertGroupPermission(It.IsAny<Guid>()));

        LogicalAccountRepositoryMock = new Mock<ILogicalAccountRepository>();

        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CurrencyConverterMock.Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns<decimal, string>((amount, _) => amount);

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<ILogicalAccountRepository> LogicalAccountRepositoryMock { get; }

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
