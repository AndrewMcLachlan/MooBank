using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Models.Config;
using Asm.Security;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Asm.MooBank.Security;

internal class GraphUserDataProvider : IUserDataProvider
{
    public const string UserIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string GraphScope = "https://graph.microsoft.com/.default";

    private readonly IPrincipalProvider _principalProvider;
    private readonly GraphConfig _graphConfig;

    public GraphUserDataProvider(IPrincipalProvider principalProvider, IOptions<GraphConfig> graphConfigOptions)
    {
        _principalProvider = principalProvider;
        _graphConfig = graphConfigOptions.Value;
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

        var user = await graphClient.Users[id.ToString()].GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Select = new string[] { "displayName", "givenName", "surName", "mail" };
        });

        if (user == null) throw new NotFoundException("User not found");

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
        var scopes = new string[] { "https://graph.microsoft.com/.default" };

        TokenCredentialOptions options = new()
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };


        ClientSecretCredential clientSecretCredential = new(_graphConfig.TenantId, _graphConfig.ClientId, _graphConfig.ClientSecret, options);

        GraphServiceClient graphClient = new(clientSecretCredential, scopes);

        return graphClient;
    }
}
