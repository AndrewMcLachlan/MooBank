using Asm.MooBank.Abs;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAbs(this IServiceCollection services)
    {
        services.AddHttpClient("abs", options =>
        {
            options.BaseAddress = new Uri("https://data.api.abs.gov.au/rest/data/");
            options.DefaultRequestHeaders.Add("Accept", "application/xml");
            options.DefaultRequestHeaders.Add("User-Agent", "Asm.MooBank/3.0");
        });
        services.AddScoped<IAbsClient, AbsClient>();

        return services;
    }
}
