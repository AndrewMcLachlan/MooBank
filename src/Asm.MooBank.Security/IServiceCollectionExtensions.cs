using Asm.MooBank.Security;
using Asm.MooBank.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUserDataProvider(this IServiceCollection services) =>
        services.AddScoped<IUserIdProvider, GraphUserDataProvider>()
                .AddScoped<IUserDataProvider, GraphUserDataProvider>();

}
