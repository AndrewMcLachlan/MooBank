using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Asm.EntityFrameworkCore
{
    /// <summary>
    /// A connection interceptor that retrieves a managed service identity token for database authentication
    /// </summary>
    /// <remarks>
    /// Based on https://stackoverflow.com/questions/54187241/ef-core-connection-to-azure-sql-with-managed-identity.
    /// </remarks>
    public class AzureAuthenticationInterceptor : DbConnectionInterceptor
    {
        private const string ManagedServiceIdentityEnvVariable = "ManagedServiceIdentity";
        private const string RequiredEnvVariableValue = "false";

        private const string AzureDatabaseResourceIdentifier = "https://database.windows.net";

        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IHostEnvironment _hostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAuthenticationInterceptor"/> class.
        /// </summary>
        /// <param name="azureServiceTokenProvider">An Azure Service Token Provider instance.</param>
        public AzureAuthenticationInterceptor(AzureServiceTokenProvider azureServiceTokenProvider, IHostEnvironment hostEnvironment) : base()
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _hostEnvironment = hostEnvironment;
        }

        /// <inheritdoc />
        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            if (!_hostEnvironment.IsDevelopment() && Environment.GetEnvironmentVariable(ManagedServiceIdentityEnvVariable) != RequiredEnvVariableValue && connection is SqlConnection sqlConnection)
            {
                sqlConnection.AccessToken = await GetAccessToken();
            }
            return result;
        }

        /// <inheritdoc />
        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            if (!_hostEnvironment.IsDevelopment() && Environment.GetEnvironmentVariable(ManagedServiceIdentityEnvVariable) != RequiredEnvVariableValue && connection is SqlConnection sqlConnection)
            {
                sqlConnection.AccessToken = GetAccessToken().Result;
            }
            return result;
        }

        private Task<string> GetAccessToken() => _azureServiceTokenProvider.GetAccessTokenAsync(AzureDatabaseResourceIdentifier);
    }
}
