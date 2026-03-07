using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Security;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUserDataProvider(this IServiceCollection services) =>
         services.AddScoped<SettableUserDataProvider>()
                 .AddScoped<IUserIdProvider>(sp => sp.GetRequiredService<SettableUserDataProvider>())
                 .AddScoped<IUserDataProvider>(sp => sp.GetRequiredService<SettableUserDataProvider>())
                 .AddScoped<ISettableUserDataProvider>(sp => sp.GetRequiredService<SettableUserDataProvider>());

    public static IServiceCollection AddAuthorisationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, InstrumentOwnerAuthorisationHandler>();
        services.AddScoped<IAuthorizationHandler, InstrumentViewerAuthorisationHandler>();
        services.AddScoped<IAuthorizationHandler, FamilyMemberAuthorisationHandler>();
        services.AddScoped<IAuthorizationHandler, GroupOwnerAuthorisationHandler>();

        return services;
    }
}
