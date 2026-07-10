using Asm.Domain.Infrastructure;
using Asm.MooBank.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Tests.Support;

/// <summary>
/// Factory for creating in-memory database contexts for testing.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory MooBankContext for testing.
    /// </summary>
    public static MooBankContext Create(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var publisher = new Mock<IPublisher>();
        var context = new MooBankContext(options, publisher.Object);

        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a new in-memory MooBankContext scoped to the given user, so the tenant
    /// and soft-delete query filters behave as they do at runtime.
    /// </summary>
    public static MooBankContext Create(Models.User user, string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var publisher = new Mock<IPublisher>();
        var userDataProvider = new Mock<Security.IUserDataProvider>();
        userDataProvider.Setup(p => p.GetCurrentUser()).Returns(user);
        userDataProvider.Setup(p => p.CurrentUserId).Returns(user.Id);

        var context = new MooBankContext(options, publisher.Object, userDataProvider.Object);

        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a test user model for repository tests.
    /// </summary>
    public static Models.User CreateTestUser(Guid? familyId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = familyId ?? Guid.NewGuid(),
            Currency = "AUD",
        };
}
