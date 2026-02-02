#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Security;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Families.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        FamilyRepositoryMock = new Mock<IFamilyRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        SecurityMock = new Mock<ISecurity>();

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IFamilyRepository> FamilyRepositoryMock { get; }

    public Mock<IUserRepository> UserRepositoryMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

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
