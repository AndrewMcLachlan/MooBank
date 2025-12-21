using Asm.MooBank.Eodhd;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEodhd(this IServiceCollection services, Action<EodhdConfig> options)
    {
        services.Configure(options);

        services.AddHttpClient("eodhd")
            .AddStandardResilienceHandler();
        services.AddScoped<IStockPriceClient, StockPriceClient>();

        return services;
    }
}
