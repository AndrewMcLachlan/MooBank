using Asm.MooBank.Audit;
using Asm.MooBank.Models;

namespace Asm.MooBank.Api.Middleware;

public class AuditMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (!AuditedMethods.Contains(context.Request.Method)) return;
        if (context.User.Identity?.IsAuthenticated != true) return;

        var audit = context.RequestServices.GetRequiredService<IAuditLogger>();
        var user = context.RequestServices.GetRequiredService<User>();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        audit.HttpMutation(user, context.Request.Method, context.Request.Path.Value!, ipAddress, context.Response.StatusCode);
    }
}
