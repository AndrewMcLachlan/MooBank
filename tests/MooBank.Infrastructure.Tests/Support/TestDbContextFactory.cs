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

    public static MooBankContext CreateWithPublisher(string? databaseName, Mock<IPublisher> publisherMock)
    {
        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new MooBankContext(options, publisherMock.Object);
    }
}
