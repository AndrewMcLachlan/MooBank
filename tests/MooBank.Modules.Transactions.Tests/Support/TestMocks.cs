using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Security;

namespace Asm.MooBank.Modules.Transactions.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        TransactionRepositoryMock = new Mock<ITransactionRepository>();
        InstrumentRepositoryMock = new Mock<IInstrumentRepository>();
        TagRepositoryMock = new Mock<ITagRepository>();
        UserIdProviderMock = new Mock<IUserIdProvider>();

        var userId = Guid.NewGuid();
        UserIdProviderMock.Setup(u => u.CurrentUserId).Returns(userId);

        User = CreateTestUser(id: userId);
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<ITransactionRepository> TransactionRepositoryMock { get; }

    public Mock<IInstrumentRepository> InstrumentRepositoryMock { get; }

    public Mock<ITagRepository> TagRepositoryMock { get; }

    public Mock<IUserIdProvider> UserIdProviderMock { get; }

    public User User { get; private set; }

    public void SetUser(User user)
    {
        User = user;
        UserIdProviderMock.Setup(u => u.CurrentUserId).Returns(user.Id);
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
