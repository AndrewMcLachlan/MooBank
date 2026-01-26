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
    public static MooBankContext Create(string databaseName = null)
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
