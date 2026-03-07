using System.Security.Claims;
using Asm.MooBank.Models;
using Asm.Security;

namespace Asm.MooBank.Security;

public class ClaimsUserDataProvider(IPrincipalProvider principalProvider) : IUserDataProvider
{
    public Guid CurrentUserId => principalProvider.Principal?.GetClaimValue<Guid>(ClaimTypes.UserId) ?? Guid.Empty;

    public User GetCurrentUser()
    {
        if (principalProvider.Principal == null) throw new InvalidOperationException("There is no current user");

        if (principalProvider.Principal.Identity?.IsAuthenticated != true) return null!;

        return new()
        {
            Id = CurrentUserId,
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
}
