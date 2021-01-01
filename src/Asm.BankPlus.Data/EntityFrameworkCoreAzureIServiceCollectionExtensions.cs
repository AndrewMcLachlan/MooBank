using Asm.EntityFrameworkCore;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> class.
    /// </summary>
    public static class EntityFrameworkCoreAzureIServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a support for managed service identity authentication of SQL Server connections.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> object that this method extends.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddManagedServiceIdentityForSqlServer(this IServiceCollection services)
        {
            return services.AddSingleton((serviceProvider) => new AzureAuthenticationInterceptor(new AzureServiceTokenProvider(), serviceProvider.GetRequiredService<IHostEnvironment>()));
        }
    }
}
