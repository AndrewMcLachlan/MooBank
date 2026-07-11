#nullable enable
using Asm.Domain;
using Asm.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure.Tests.Support;

internal static class TestDbContextFactory
{
    public static MooBankContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var publisherMock = new Mock<IPublisher>();
        return new MooBankContext(options, publisherMock.Object);
    }

    /// <summary>
    /// Creates a new in-memory MooBankContext scoped to the given user, so the tenant
    /// and soft-delete query filters behave as they do at runtime.
    /// </summary>
    public static MooBankContext Create(Models.User user, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var publisherMock = new Mock<IPublisher>();
        var userDataProviderMock = new Mock<Security.IUserDataProvider>();
        userDataProviderMock.Setup(p => p.GetCurrentUser()).Returns(user);
        userDataProviderMock.Setup(p => p.CurrentUserId).Returns(user.Id);

        return new MooBankContext(options, publisherMock.Object, userDataProviderMock.Object);
    }

    public static MooBankContext CreateWithPublisher(string? databaseName, Mock<IPublisher> publisherMock)
    {
        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new MooBankContext(options, publisherMock.Object);
    }
}
