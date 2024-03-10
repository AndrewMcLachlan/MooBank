
using System.Security.Claims;

namespace Asm.MooBank.Security;
public static class ClaimsPrincipalExtensions
{
    public static T? GetClaimValue<T>(this ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.FindFirst(claimType);
        if (claim == null) return default;

        return typeof(T) switch
        {
            var type when type == typeof(Guid) => (T)Convert.ChangeType(Guid.Parse(claim.Value), typeof(T)),
            var type when type == typeof(Guid?) => claim.Value is null ? default : (T)Convert.ChangeType(Guid.Parse(claim.Value), typeof(Guid)),
            var type when type == typeof(int) => (T)Convert.ChangeType(Int32.Parse(claim.Value), typeof(T)),
            _ => (T)Convert.ChangeType(claim.Value, typeof(T)),
        };
    }
}
