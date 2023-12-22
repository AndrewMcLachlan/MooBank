using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Security;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUserDataProvider(this IServiceCollection services) =>
         services.AddScoped<IUserIdProvider, ClaimsUserDataProvider>()
                 .AddScoped<IUserDataProvider, ClaimsUserDataProvider>();

    public static IServiceCollection AddAuthorisationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, AccountHolderAuthorisationHandler>();
        services.AddScoped<IAuthorizationHandler, AccountViewerAuthorisationHandler>();
        services.AddScoped<IAuthorizationHandler, FamilyMemberAuthorisationHandler>();

        return services;
    }
}
