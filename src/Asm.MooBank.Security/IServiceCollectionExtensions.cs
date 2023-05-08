using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Security;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUserDataProvider(this IServiceCollection services) =>
        services.AddScoped<IUserIdProvider, GraphUserDataProvider>()
                .AddScoped<IUserDataProvider, GraphUserDataProvider>();

}
