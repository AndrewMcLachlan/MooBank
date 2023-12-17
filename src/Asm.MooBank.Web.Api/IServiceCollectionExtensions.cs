using System.Security.Claims;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Asm.MooBank.Web.Api;

public static class IServiceCollectionExtensions
{
    public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, IConfiguration configuration) =>
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddAzureADBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error(context.Exception, "Authentication failed.");
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;

                if (principal?.Identity is ClaimsIdentity identity)
                {
                    Guid userId = context.Principal!.GetClaimValue<Guid>(Security.ClaimTypes.UserId);
                    var dataContext = context.HttpContext.RequestServices.GetRequiredService<MooBankContext>();

                    var user = await dataContext.AccountHolders.Include(ah => ah.AccountAccountHolders).ThenInclude(aah => aah.Account).AsNoTracking().SingleOrDefaultAsync(ah => ah.AccountHolderId == userId);

                    if (user == null)
                    {
                        context.Fail("User does not exist in the database.");
                        return;
                    }

                    var claims = user.Accounts.Select(a => new Claim(Security.ClaimTypes.AccountId, a.Id.ToString())).ToList();

                    if (user.PrimaryAccountId != null) claims.Add(new Claim(Security.ClaimTypes.PrimaryAccountId, user.PrimaryAccountId.Value.ToString()));
                    claims.Add(new Claim(Security.ClaimTypes.FamilyId, user.FamilyId.ToString()));

                    principal.AddIdentity(new(claims));
                }
            }
        };

        configuration.Bind("OAuth", options.AzureOAuthOptions);
    });
}
