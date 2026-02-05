#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using MooBankClaimTypes = Asm.MooBank.Security.ClaimTypes;

namespace Asm.MooBank.Web.Api.Tests.Infrastructure;

/// <summary>
/// A fake policy evaluator for integration tests that authenticates using test headers
/// and evaluates authorization based on claims.
/// </summary>
public class FakePolicyEvaluator : IPolicyEvaluator
{
    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        // Check if test headers are present
        if (!context.Request.Headers.TryGetValue(TestAuthHandler.UserIdHeader, out var userIdValues) ||
            String.IsNullOrEmpty(userIdValues.FirstOrDefault()))
        {
            return AuthenticateResult.NoResult();
        }

        var userId = userIdValues.First()!;
        var claims = BuildClaims(context, userId);

        var identity = new ClaimsIdentity(claims, "FakeScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "FakeScheme");

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public async Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
    {
        if (!authenticationResult.Succeeded)
        {
            return PolicyAuthorizationResult.Challenge();
        }

        // For authorization policies that check route params (InstrumentViewer, InstrumentOwner, GroupOwner),
        // we need to evaluate them based on claims

        var principal = authenticationResult.Principal!;
        var requirements = policy.Requirements;

        foreach (var requirement in requirements)
        {
            // Check for instrument-based requirements
            var requirementType = requirement.GetType().Name;

            if (requirementType.Contains("Instrument"))
            {
                // Get the instrument ID from the route
                var instrumentId = GetRouteValue(context, "instrumentId")
                    ?? GetRouteValue(context, "id")
                    ?? GetRouteValue(context, "accountId");

                if (instrumentId == null)
                    continue;

                // Check if user has access to this instrument
                var accountIds = principal.Claims
                    .Where(c => c.Type == MooBankClaimTypes.AccountId)
                    .Select(c => c.Value)
                    .ToList();

                var sharedAccountIds = principal.Claims
                    .Where(c => c.Type == MooBankClaimTypes.SharedAccountId)
                    .Select(c => c.Value)
                    .ToList();

                // For InstrumentViewer, check both owned and shared
                if (requirementType.Contains("Viewer"))
                {
                    if (!accountIds.Contains(instrumentId) && !sharedAccountIds.Contains(instrumentId))
                    {
                        return PolicyAuthorizationResult.Forbid();
                    }
                }
                // For InstrumentOwner, check only owned
                else if (requirementType.Contains("Owner"))
                {
                    if (!accountIds.Contains(instrumentId))
                    {
                        return PolicyAuthorizationResult.Forbid();
                    }
                }
            }
            else if (requirementType.Contains("Group"))
            {
                // Get the group ID from the route
                var groupId = GetRouteValue(context, "groupId");

                if (groupId == null)
                    continue;

                var groupIds = principal.Claims
                    .Where(c => c.Type == MooBankClaimTypes.GroupId)
                    .Select(c => c.Value)
                    .ToList();

                if (!groupIds.Contains(groupId))
                {
                    return PolicyAuthorizationResult.Forbid();
                }
            }
            else if (requirement is RolesAuthorizationRequirement rolesRequirement)
            {
                // Check for role requirements (e.g., Admin)
                var userRoles = principal.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (!rolesRequirement.AllowedRoles.Any(r => userRoles.Contains(r)))
                {
                    return PolicyAuthorizationResult.Forbid();
                }
            }
        }

        return await Task.FromResult(PolicyAuthorizationResult.Success());
    }

    private static List<Claim> BuildClaims(HttpContext context, string userId)
    {
        var email = GetHeaderValue(context, TestAuthHandler.UserEmailHeader, $"{userId}@test.com");
        var familyId = GetHeaderValue(context, TestAuthHandler.FamilyIdHeader, Guid.NewGuid().ToString());
        var currency = GetHeaderValue(context, TestAuthHandler.CurrencyHeader, "AUD");

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
        AddMultiValueClaims(context, claims, TestAuthHandler.AccountIdsHeader, MooBankClaimTypes.AccountId);

        // Add shared account IDs
        AddMultiValueClaims(context, claims, TestAuthHandler.SharedAccountIdsHeader, MooBankClaimTypes.SharedAccountId);

        // Add group IDs
        AddMultiValueClaims(context, claims, TestAuthHandler.GroupIdsHeader, MooBankClaimTypes.GroupId);

        // Add roles
        if (context.Request.Headers.TryGetValue(TestAuthHandler.RolesHeader, out var rolesValues))
        {
            var roles = rolesValues.FirstOrDefault()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }
        }

        return claims;
    }

    private static string GetHeaderValue(HttpContext context, string header, string defaultValue)
    {
        return context.Request.Headers.TryGetValue(header, out var values) && !String.IsNullOrEmpty(values.FirstOrDefault())
            ? values.First()!
            : defaultValue;
    }

    private static void AddMultiValueClaims(HttpContext context, List<Claim> claims, string header, string claimType)
    {
        if (context.Request.Headers.TryGetValue(header, out var values))
        {
            var ids = values.FirstOrDefault()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            foreach (var id in ids)
            {
                claims.Add(new Claim(claimType, id.Trim()));
            }
        }
    }

    private static string? GetRouteValue(HttpContext context, string key)
    {
        return context.Request.RouteValues.TryGetValue(key, out var value)
            ? value?.ToString()
            : null;
    }
}
