using System.Security.Claims;
using Asm.Security;

namespace Asm.MooBank.Web.Jobs;

internal class PrincipalProvider : IPrincipalProvider
{
    public ClaimsPrincipal? Principal => throw new NotSupportedException();
}
