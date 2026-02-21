#nullable enable
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MooBankClaimTypes = Asm.MooBank.Security.ClaimTypes;

namespace Asm.MooBank.Web.Api.Tests.Infrastructure;

/// <summary>
/// A fake authentication handler for integration tests that creates claims from test headers.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";

    // Headers for test configuration
    public const string UserIdHeader = "X-Test-UserId";
    public const string UserEmailHeader = "X-Test-UserEmail";
    public const string FamilyIdHeader = "X-Test-FamilyId";
    public const string AccountIdsHeader = "X-Test-AccountIds";
    public const string SharedAccountIdsHeader = "X-Test-SharedAccountIds";
    public const string GroupIdsHeader = "X-Test-GroupIds";
    public const string CurrencyHeader = "X-Test-Currency";
    public const string RolesHeader = "X-Test-Roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if test headers are present (UserId is required for auth)
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues) ||
            String.IsNullOrEmpty(userIdValues.FirstOrDefault()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValues.First()!;
        var email = GetHeaderValue(UserEmailHeader, $"{userId}@test.com");
        var familyId = GetHeaderValue(FamilyIdHeader, Guid.NewGuid().ToString());
        var currency = GetHeaderValue(CurrencyHeader, "AUD");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new("sub", userId),
            new(MooBankClaimTypes.UserId, userId),
            new(MooBankClaimTypes.FamilyId, familyId),
            new(MooBankClaimTypes.Currency, currency),
        };

        // Add account IDs
        AddMultiValueClaims(claims, AccountIdsHeader, MooBankClaimTypes.AccountId);

        // Add shared account IDs
        AddMultiValueClaims(claims, SharedAccountIdsHeader, MooBankClaimTypes.SharedAccountId);

        // Add group IDs
        AddMultiValueClaims(claims, GroupIdsHeader, MooBankClaimTypes.GroupId);

        // Add roles
        if (Request.Headers.TryGetValue(RolesHeader, out var rolesValues))
        {
            var roles = rolesValues.FirstOrDefault()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string GetHeaderValue(string header, string defaultValue)
    {
        return Request.Headers.TryGetValue(header, out var values) && !String.IsNullOrEmpty(values.FirstOrDefault())
            ? values.First()!
            : defaultValue;
    }

    private void AddMultiValueClaims(List<Claim> claims, string header, string claimType)
    {
        if (Request.Headers.TryGetValue(header, out var values))
        {
            var ids = values.FirstOrDefault()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            foreach (var id in ids)
            {
                claims.Add(new Claim(claimType, id.Trim()));
            }
        }
    }
}
