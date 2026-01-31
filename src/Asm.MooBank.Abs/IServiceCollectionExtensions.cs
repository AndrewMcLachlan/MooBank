using Asm.MooBank.Abs;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAbs(this IServiceCollection services)
    {
        services.AddHttpClient("abs", options =>
        {
            options.BaseAddress = new Uri("https://data.api.abs.gov.au/rest/data/");
            options.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/xml");
            options.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Asm.MooBank/3.0");
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IAbsClient, AbsClient>();

        return services;
    }
}
