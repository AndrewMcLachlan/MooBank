using Asm.MooBank.Eodhd;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEodhd(this IServiceCollection services, Action<EodhdConfig> options)
    {
        services.Configure(options);

        services.AddHttpClient("eodhd", options =>
        {
            options.BaseAddress = new("https://eodhd.com/api/eod/");
            options.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Asm.MooBank/3.0");
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IStockPriceClient, StockPriceClient>();

        return services;
    }
}
