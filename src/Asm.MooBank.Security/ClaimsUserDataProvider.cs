using Asm.MooBank.Models;
using Asm.Security;

namespace Asm.MooBank.Security;

public class ClaimsUserDataProvider : IUserDataProvider
{
    private readonly IPrincipalProvider _principalProvider;
    public ClaimsUserDataProvider(IPrincipalProvider principalProvider)
    {
        _principalProvider = principalProvider;
    }

    public Guid CurrentUserId => _principalProvider.Principal?.GetClaimValue<Guid>(ClaimTypes.UserId) ?? Guid.Empty;

    public Task<AccountHolder> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(GetCurrentUser());

    public AccountHolder GetCurrentUser()
    {
        if (_principalProvider.Principal == null) throw new InvalidOperationException("There is no current user");

        return new()
        {
            Id = CurrentUserId,
            EmailAddress = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Email),
            FirstName = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.GivenName),
            LastName = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Surname),
            Accounts = _principalProvider.Principal.Claims.Where(c => c.Type == ClaimTypes.AccountId).Select(c => c.Value).Select(Guid.Parse).ToList(),
            FamilyId = _principalProvider.Principal.GetClaimValue<Guid>(ClaimTypes.FamilyId),
            PrimaryAccountId = _principalProvider.Principal.GetClaimValue<Guid?>(ClaimTypes.PrimaryAccountId),
        };
    }
}
