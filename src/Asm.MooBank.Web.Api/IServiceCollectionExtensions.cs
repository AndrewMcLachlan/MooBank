using System.Security.Claims;
using Asm.MooBank.Domain.Entities.Account;
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

                    var user = await dataContext.AccountHolders.Include(ah => ah.AccountAccountHolders).ThenInclude(aah => aah.Account).AsNoTracking().SingleOrDefaultAsync(ah => ah.Id == userId);

                    if (user == null)
                    {
                        context.Fail("User does not exist in the database.");
                        return;
                    }

                    var shared = await dataContext.Set<Instrument>().Where(a => a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == user.FamilyId)).Select(a => a.Id).ToListAsync();

                    var claims = user.Accounts.Select(a => new Claim(Security.ClaimTypes.AccountId, a.Id.ToString())).ToList();
                    claims.AddRange(shared.Select(a => new Claim(Security.ClaimTypes.SharedAccountId, a.ToString())).ToList());

                    if (user.PrimaryAccountId != null) claims.Add(new Claim(Security.ClaimTypes.PrimaryAccountId, user.PrimaryAccountId.Value.ToString()));
                    claims.Add(new Claim(Security.ClaimTypes.FamilyId, user.FamilyId.ToString()));
                    claims.Add(new Claim(Security.ClaimTypes.Currency, user.Currency));

                    principal.AddIdentity(new(claims));
                }
            }
        };

        configuration.Bind("OAuth", options.AzureOAuthOptions);
    });
}
