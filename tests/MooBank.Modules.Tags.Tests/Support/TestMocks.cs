using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        TagRepositoryMock = new Mock<ITagRepository>();
        SecurityMock = new Mock<ISecurity>();

        User = CreateTestUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<ITagRepository> TagRepositoryMock { get; }

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

    public static IEnumerable<TagRelationship> CreateEmptyTagRelationships() => [];

    public static IEnumerable<TagRelationship> CreateTagRelationships(params (DomainTag tag, DomainTag parentTag)[] relationships)
    {
        return relationships.Select(r => new TagRelationship { Tag = r.tag, ParentTag = r.parentTag });
    }
}
