using System.Security.Claims;
using Asm.AspNetCore.Authentication;
using Asm.MooBank.Infrastructure;
using Asm.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Serilog;

namespace Asm.MooBank.Web.Api;

public static class IServiceCollectionExtensions
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
    };

    public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, IConfiguration configuration) =>
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddStandardJwtBearer(options =>
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

                    var cache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();

                    var claims = await cache.GetOrCreateAsync(CacheKeys.UserCacheKey(userId), async ct =>
                    {
                        var dataContext = context.HttpContext.RequestServices.GetRequiredService<MooBankContext>();

                        var user = await dataContext.Set<Domain.Entities.User.User>().Include(ah => ah.InstrumentOwners).ThenInclude(aah => aah.Instrument).ThenInclude(i => i.VirtualInstruments).Include(u => u.Groups).AsNoTracking().SingleOrDefaultAsync(ah => ah.Id == userId, cancellationToken: ct);

                        List<Claim> claims = [];

                        if (user == null)
                        {
                            Domain.Entities.Family.Family family = new()
                            {
                                Name = "My Family",
                            };

                            user = new(userId)
                            {
                                EmailAddress = principal.GetClaimValue<string>(ClaimTypes.Email)!,
                                Currency = "AUD",
                                FirstName = principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.GivenName),
                                LastName = principal.GetClaimValue<string>(System.Security.Claims.ClaimTypes.Surname),
                                Family = family,
                            };

                            dataContext.Add(user);

                            await dataContext.SaveChangesAsync(ct);
                        }

                        var owned = user.Instruments.Select(i => i.Id);
                        var virtualOwned = user.Instruments.SelectMany(i => i.VirtualInstruments).Select(i => i.Id);

                        var sharedInstruments = await dataContext.Set<Domain.Entities.Instrument.Instrument>().Where(a => a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId)).Include(i => i.VirtualInstruments).ToListAsync(cancellationToken: ct);
                        var virtualShared = sharedInstruments.SelectMany(i => i.VirtualInstruments).Select(i => i.Id);

                        var instruments = owned.Union(virtualOwned);

                        claims = [.. instruments.Select(a => new Claim(Security.ClaimTypes.AccountId, a.ToString()))];

                        var allShared = sharedInstruments.Select(i => i.Id).Union(virtualShared).Except(owned);

                        claims.AddRange(allShared.Select(a => new Claim(Security.ClaimTypes.SharedAccountId, a.ToString())));

                        var groups = user.Groups.Select(i => i.Id);

                        claims.AddRange(groups.Select(a => new Claim(Security.ClaimTypes.GroupId, a.ToString())));

                        if (user.PrimaryAccountId != null) claims.Add(new Claim(Security.ClaimTypes.PrimaryAccountId, user.PrimaryAccountId.Value.ToString()));
                        claims.Add(new Claim(Security.ClaimTypes.FamilyId, user.FamilyId.ToString()));
                        claims.Add(new Claim(Security.ClaimTypes.Currency, user.Currency));

                        return claims;
                    }, CacheOptions, cancellationToken: CancellationToken.None);

                    principal.AddIdentity(new(claims));
                }
            }
        };

        AzureOAuthOptions oAuthOptions = configuration.GetSection("OAuth").Get<AzureOAuthOptions>() ?? throw new InvalidOperationException("OAuth config not defined");
        options.OAuthOptions = oAuthOptions;
    });
}
