using System.Security.Claims;
using Serilog.Context;

namespace Asm.BankPlus.Web.Middleware
{
    public class SerilogEnrichWithUserMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogEnrichWithUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IHttpContextAccessor httpContextAccessor)
        {
            string name = httpContextAccessor.HttpContext.GetUserName();

            using (LogContext.PushProperty("User", name))
            {
                await _next.Invoke(context);
            }
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class SerilogEnrichmentMiddlewareExtensions
    {
        public static IApplicationBuilder UseSerilogEnrichWithUser(this IApplicationBuilder app) =>
            app.UseMiddleware<Asm.BankPlus.Web.Middleware.SerilogEnrichWithUserMiddleware>();
    }
}
