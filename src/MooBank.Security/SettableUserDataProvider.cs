using System.Security.Claims;
using Asm.MooBank.Models;
using Asm.Security;

namespace Asm.MooBank.Security;

/// <summary>
/// A composite user data provider that supports both HTTP context and background processing.
/// When a user is explicitly set (for background processing), it uses that user.
/// Otherwise, it falls back to claims-based resolution from the HTTP context.
/// </summary>
public class SettableUserDataProvider(IPrincipalProvider principalProvider) : ISettableUserDataProvider
{
    private User? _user;

    public Guid CurrentUserId
    {
        get
        {
            if (_user != null) return _user.Id;
            return principalProvider.Principal?.GetClaimValue<Guid>(ClaimTypes.UserId) ?? Guid.Empty;
        }
    }

    public User GetCurrentUser()
    {
        // If a user has been explicitly set (background processing), use that
        if (_user != null) return _user;

        // Otherwise, fall back to claims-based resolution (HTTP context)
        if (principalProvider.Principal == null)
            throw new InvalidOperationException("There is no current user");

        if (principalProvider.Principal.Identity?.IsAuthenticated != true)
            return null!;

        return new()
        {
            Id = principalProvider.Principal.GetClaimValue<Guid>(ClaimTypes.UserId),
            EmailAddress = principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Email) ?? throw new InvalidOperationException("User must have an email"),
            FirstName = principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.GivenName),
            LastName = principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Surname),
            Accounts = [.. principalProvider.Principal.Claims.Where(c => c.Type == ClaimTypes.AccountId).Select(c => c.Value).Select(Guid.Parse)],
            SharedAccounts = [.. principalProvider.Principal.Claims.Where(c => c.Type == ClaimTypes.SharedAccountId).Select(c => c.Value).Select(Guid.Parse)],
            Groups = [.. principalProvider.Principal.Claims.Where(c => c.Type == ClaimTypes.GroupId).Select(c => c.Value).Select(Guid.Parse)],
            FamilyId = principalProvider.Principal.GetClaimValue<Guid>(ClaimTypes.FamilyId),
            PrimaryAccountId = principalProvider.Principal.GetClaimValue<Guid?>(ClaimTypes.PrimaryAccountId),
            Currency = principalProvider.Principal.GetClaimValue<string>(ClaimTypes.Currency) ?? throw new InvalidOperationException("User must have a currency"),
        };
    }

    public void SetUser(User user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }
}
