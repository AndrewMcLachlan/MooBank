using System;
using Asm.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public static class EntityFrameworkCoreAzureDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Adds support for authentication with Azure Managed Service Identity.
        /// </summary>
        /// <remarks>
        /// Requires <see cref="EntityFrameworkCoreAzureIServiceCollectionExtensions.AddManagedServiceIdentityForSqlServer(IServiceCollection)"/>.
        /// </remarks>
        /// <param name="builder">The <see cref="DbContextOptionsBuilder"/> instance that this method extends.</param>
        /// <param name="serviceProvider">A service provider instance.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder AddManagedServiceIdentity(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
        {
            return builder.AddInterceptors(serviceProvider.GetRequiredService<AzureAuthenticationInterceptor>());
        }
    }
}
