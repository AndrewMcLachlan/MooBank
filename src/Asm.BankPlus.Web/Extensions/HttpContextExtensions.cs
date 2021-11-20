using System.Security.Claims;

namespace Asm.BankPlus.Web;

public static class HttpContextExtensions
{
    public static string GetUserName(this HttpContext? context)
    {
        string name;

        if (context?.User?.Identity is ClaimsIdentity identity)
        {
            name = $"{identity.Claims.SingleOrDefault(c => c.Type == "name")?.Value} ({identity.Claims.SingleOrDefault(c => c.Type == "preferred_username")?.Value})";
        }
        else
        {
            name = "-";
        }

        return name;
    }
}
