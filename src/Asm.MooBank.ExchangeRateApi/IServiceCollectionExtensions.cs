using Asm.MooBank.ExchangeRateApi;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddExchangeRateApi(this IServiceCollection services, Action<ExchangeRateApiConfig> options)
    {
        services.Configure(options);

        services.AddHttpClient("ExchangeRateApi")
            .AddStandardResilienceHandler();
        services.AddScoped<IExchangeRateClient, ExchangeRateClient>();

        return services;
    }
}
