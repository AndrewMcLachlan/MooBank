#nullable enable
using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Guards the authorisation dependency architecture: requirement handlers depend on
/// IAuthorisationRepository (data), never on ISecurity, whose implementation consumes
/// IAuthorizationService. Resolving IAuthorizationService constructs every registered
/// handler, so a handler depending on ISecurity is a circular dependency.
/// </summary>
[Trait("Category", "Unit")]
public class AuthorisationDependencyTests
{
    /// <summary>
    /// Given the real handler and ISecurity registrations
    /// When IAuthorizationService and ISecurity are resolved
    /// Then no circular dependency occurs
    /// </summary>
    [Fact]
    public void ResolvingAuthorisationServices_DoesNotCycle()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped(_ => new User
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
        });
        services.AddScoped(_ => new Mock<IAuthorisationRepository>().Object);
        services.AddScoped(_ => new Mock<IAuditLogger>().Object);
        services.AddScoped(_ => new Mock<IPrincipalProvider>().Object);
        Asm.MooBank.Security.IServiceCollectionExtensions.AddAuthorisationHandlers(services);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act & Assert
        Assert.Null(Record.Exception(() => scope.ServiceProvider.GetRequiredService<IAuthorizationService>()));
        Assert.Null(Record.Exception(() => scope.ServiceProvider.GetRequiredService<Asm.MooBank.Security.ISecurity>()));
    }
}
