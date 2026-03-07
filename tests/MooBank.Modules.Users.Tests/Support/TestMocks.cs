#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.User;
using Microsoft.Extensions.Caching.Hybrid;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Users.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UserRepositoryMock = new Mock<IUserRepository>();
        HybridCacheMock = new Mock<HybridCache>();

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IUserRepository> UserRepositoryMock { get; }

    public Mock<HybridCache> HybridCacheMock { get; }

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
        Guid? primaryAccountId = null,
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
            PrimaryAccountId = primaryAccountId,
            Accounts = accounts ?? [],
            SharedAccounts = sharedAccounts ?? [],
            Groups = [],
        };
    }
}
