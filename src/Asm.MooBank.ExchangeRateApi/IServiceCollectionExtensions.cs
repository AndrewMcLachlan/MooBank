using Asm.MooBank.ExchangeRateApi;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddExchangeRateApi(this IServiceCollection services, Action<ExchangeRateApiConfig> options)
    {
        services.Configure(options);

        services.AddHttpClient("ExchangeRateApi", options =>
        {
            options.BaseAddress = new("https://v6.exchangerate-api.com/v6/latest/");
            options.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Asm.MooBank/3.0");
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IExchangeRateClient, ExchangeRateClient>();

        return services;
    }
}
