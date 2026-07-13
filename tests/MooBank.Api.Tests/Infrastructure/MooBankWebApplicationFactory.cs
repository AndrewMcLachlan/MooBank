#nullable enable
using Asm.MooBank.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Asm.MooBank.Api.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing with fake authentication and in-memory database.
/// </summary>
public class MooBankWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"MooBankTest_{Guid.NewGuid()}";

    // Holds the in-memory store outside the EF internal service provider so the seeding context and
    // the request-scoped contexts share the same data.
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace the app's SqlServer DbContext with an in-memory one. Removing the options object
            // is not enough: AddDbContext also registers an IDbContextOptionsConfiguration<MooBankContext>
            // (the app's UseSqlServer action) that would otherwise run alongside our UseInMemoryDatabase,
            // leaving both providers on the options ("Only a single database provider can be registered",
            // and — more subtly — writes and reads landing in different stores). Remove it too.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<MooBankContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(MooBankContext)
                         || d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal))
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // A shared InMemoryDatabaseRoot keeps one store per database name across the seeding context
            // and the request-scoped contexts.
            services.AddDbContext<MooBankContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName, _databaseRoot);
            });

            // Authenticate via test headers, but leave the REAL authorization pipeline in place so
            // the production requirement handlers execute (against seeded data / claims). The app
            // explicitly sets the JwtBearer authenticate/challenge schemes, so override every default
            // to TestScheme (setting DefaultScheme alone is not enough).
            // The authorization policies pin the JwtBearer ("Bearer") authentication scheme, so the
            // policy evaluator authenticates against it regardless of the default scheme. Swap that
            // scheme's handler for the test handler, so the REAL authorization requirement handlers
            // execute over claims supplied via test headers (see TestAuthHandler).
            services.AddTransient<TestAuthHandler>();
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                if (options.SchemeMap.TryGetValue(JwtBearerDefaults.AuthenticationScheme, out var bearer))
                {
                    bearer.HandlerType = typeof(TestAuthHandler);
                }
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Creates a client with test user authentication headers configured.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(TestUser user)
    {
        var client = CreateClient();
        ConfigureClientAuth(client, user);
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated client (no auth headers).
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Configures an existing HttpClient with test user authentication headers.
    /// </summary>
    public static void ConfigureClientAuth(HttpClient client, TestUser user)
    {
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserEmailHeader, user.Email);
        client.DefaultRequestHeaders.Add(TestAuthHandler.FamilyIdHeader, user.FamilyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CurrencyHeader, user.Currency);

        if (user.AccountIds.Count > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.AccountIdsHeader,
                String.Join(",", user.AccountIds));
        }

        if (user.SharedAccountIds.Count > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.SharedAccountIdsHeader,
                String.Join(",", user.SharedAccountIds));
        }

        if (user.GroupIds.Count > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.GroupIdsHeader,
                String.Join(",", user.GroupIds));
        }

        if (user.Roles.Count > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader,
                String.Join(",", user.Roles));
        }
    }

    /// <summary>
    /// Gets a scoped service provider for seeding data.
    /// </summary>
    public IServiceScope CreateScope() => Services.CreateScope();

    /// <summary>
    /// Seeds test data using the provided action.
    /// </summary>
    public async Task SeedDataAsync(Func<MooBankContext, Task> seedAction)
    {
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MooBankContext>();
        await context.Database.EnsureCreatedAsync();
        await seedAction(context);
    }
}

/// <summary>
/// Represents a test user for authentication purposes.
/// </summary>
public class TestUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "test@example.com";
    public Guid FamilyId { get; set; } = Guid.NewGuid();
    public string Currency { get; set; } = "AUD";
    public List<Guid> AccountIds { get; set; } = [];
    public List<Guid> SharedAccountIds { get; set; } = [];
    public List<Guid> GroupIds { get; set; } = [];
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Creates a user who owns the specified account.
    /// </summary>
    public static TestUser WithAccount(Guid accountId, Guid? familyId = null)
    {
        return new TestUser
        {
            AccountIds = [accountId],
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    /// <summary>
    /// Creates a user with shared access to the specified account.
    /// </summary>
    public static TestUser WithSharedAccount(Guid accountId, Guid? familyId = null)
    {
        return new TestUser
        {
            SharedAccountIds = [accountId],
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    /// <summary>
    /// Creates an admin user.
    /// </summary>
    public static TestUser Admin()
    {
        return new TestUser
        {
            Roles = ["Admin"],
        };
    }

    /// <summary>
    /// Creates a user who owns the specified group.
    /// </summary>
    public static TestUser WithGroup(Guid groupId, Guid? familyId = null)
    {
        return new TestUser
        {
            GroupIds = [groupId],
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    /// <summary>
    /// Creates a user who owns both an account and a group.
    /// </summary>
    public static TestUser WithAccountAndGroup(Guid accountId, Guid groupId, Guid? familyId = null)
    {
        return new TestUser
        {
            AccountIds = [accountId],
            GroupIds = [groupId],
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }
}
