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

    public ValueTask<AccountHolder> GetCurrentUser()
    {
        if (_principalProvider.Principal == null) throw new InvalidOperationException("There is no current user");

        var familyId = _principalProvider.Principal.GetClaimValue<Guid>(ClaimTypes.FamilyId);

        return ValueTask.FromResult(new AccountHolder()
        {
            Id = CurrentUserId,
            EmailAddress = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Email),
            FirstName = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.GivenName),
            LastName = _principalProvider.Principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Surname),
            Accounts = _principalProvider.Principal.Claims.Where(c => c.Type == ClaimTypes.AccountId).Select(c => c.Value).Select(Guid.Parse).ToList(),
            FamilyId = familyId,
        });
    }
}
