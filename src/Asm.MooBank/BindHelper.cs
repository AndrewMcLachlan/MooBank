using Asm.MooBank.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank;

/// <summary>
/// Helps bind commands that include an instrument ID in the route.
/// </summary>
public static class BindHelper
{
    public static ValueTask<TCommand> BindWithInstrumentIdAsync<TCommand>(HttpContext httpContext) where TCommand : InstrumentIdCommand =>
        BindWithInstrumentIdAsync<TCommand>("instrumentId", httpContext);

    public static async ValueTask<TCommand> BindWithInstrumentIdAsync<TCommand>(string paramName, HttpContext httpContext) where TCommand : InstrumentIdCommand
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues[paramName] as string, out Guid instrumentId)) throw new BadHttpRequestException("Invalid instrument ID");

        var command = await System.Text.Json.JsonSerializer.DeserializeAsync<TCommand>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return command! with { InstrumentId = instrumentId };
    }
}
