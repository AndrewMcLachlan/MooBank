using System.Security.Claims;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

public static class AuthorisationExtensions
{
    public static async Task AssertInstrumentViewer(this IAuthorizationService authorizationService, ClaimsPrincipal user, Guid instrumentId)
    {
        var result = await authorizationService.AuthorizeAsync(user, instrumentId, new InstrumentViewerRequirement());

        if (!result.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this instrument.");
        }
    }
}
