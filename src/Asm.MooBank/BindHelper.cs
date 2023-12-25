using Asm.MooBank.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank;

/// <summary>
/// Helps bind commands that include an account ID in the route.
/// </summary>
public static class BindHelper
{
    public static async ValueTask<TCommand> BindWithAccountIdAsync<TCommand>(HttpContext httpContext) where TCommand : AccountIdCommand
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("Invalid account ID");

        var command = await System.Text.Json.JsonSerializer.DeserializeAsync<TCommand>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return command! with { AccountId = accountId };
    }
}
