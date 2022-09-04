using System.Net.Http.Headers;
using Asm.MooBank.Data.Entities;
using Asm.MooBank.Security;
using Asm.Security;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Asm.MooBank.Services
{
    public class GraphUserDataProvider : IUserDataProvider
    {
        public const string UserIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        private readonly IPrincipalProvider _principalProvider;

        public GraphUserDataProvider(IPrincipalProvider principalProvider)
        {
            _principalProvider = principalProvider;
        }

        public Guid CurrentUserId
        {
            get
            {
                var id = _principalProvider.Principal?.Claims.Where(c => c.Type == UserIdClaimType).Select(c => c.Value).SingleOrDefault();

                if (id == null)
                {
                    throw new InvalidOperationException("Not correctly logged in");
                }

                return Guid.Parse(id);
            }
        }

        public async Task<AccountHolder> GetCurrentUser()
        {
            return await GetUser(CurrentUserId);
        }

        public async Task<AccountHolder> GetUser(Guid id)
        {
            GraphServiceClient graphClient = CreateServiceClient();

            var user = await graphClient.Users[id.ToString()].Request().Select("displayName,givenName,surName,mail").GetAsync();

            return new AccountHolder
            {
                AccountHolderId = id,
                FirstName = user.GivenName,
                LastName = user.Surname,
                EmailAddress = user.Mail,
            };
        }

        private GraphServiceClient CreateServiceClient()
        {

            var authenticationProvider = new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    var token = await GetApplicationAccessTokenAsync(new List<string>() { "https://graph.microsoft.com/.default" });
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                });

            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }

        public async Task<string> GetApplicationAccessTokenAsync(IList<string> scopes)
        {
            IConfidentialClientApplication app = GetConfidentialClientApp();
            AuthenticationResult authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return authResult?.AccessToken;
        }

        private IConfidentialClientApplication GetConfidentialClientApp()
        {
            IConfidentialClientApplication app;
            app = ConfidentialClientApplicationBuilder
                    .Create("045f8afa-70f2-4700-ab75-77ac41b306f7")
                    .WithClientSecret("gvwLXheN2Ba2OKFE*AxKi9jupyNq6.]+")
                    .WithAuthority(AzureCloudInstance.AzurePublic, "30efefb9-9034-4e0c-8c69-17f4578f5924")
                    .Build();

            return app;
        }
    }
}
