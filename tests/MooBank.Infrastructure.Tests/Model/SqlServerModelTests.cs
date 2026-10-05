#nullable enable
using Asm.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Asm.MooBank.Infrastructure.Tests.Model;

/// <summary>
/// Unit tests for the SQL Server mapping of the <see cref="MooBankContext"/> model.
/// </summary>
[Trait("Category", "Unit")]
public class SqlServerModelTests
{
    /// <summary>
    /// Given the MooBank model built for SQL Server
    /// When the model is finalised
    /// Then no decimal property falls back to EF's default store type
    /// </summary>
    [Fact]
    public void Model_EveryDecimalProperty_HasAStoreType()
    {
        List<string> unconfigured = [];

        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseSqlServer("Server=unused")
            .EnableServiceProviderCaching(false)
            .LogTo(
                (eventId, _) => eventId == SqlServerEventId.DecimalTypeDefaultWarning,
                eventData => unconfigured.Add(eventData is PropertyEventData p ? $"{p.Property.DeclaringType.DisplayName()}.{p.Property.Name}" : eventData.ToString()))
            .Options;

        using var context = new MooBankContext(options, new Mock<IPublisher>().Object);

        _ = context.Model;

        Assert.True(unconfigured.Count == 0, String.Join(Environment.NewLine, unconfigured));
    }
}
