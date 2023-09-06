using System.Security.Claims;
using System.Text.Json.Serialization;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Ing;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace Asm.MooBank.Web;

public class Startup
{
    private const string SpaRoot = "MooBankApp";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddProblemDetailsFactory();

        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = $"{SpaRoot}/dist";
        });

        services.AddMooBankDbContext(Configuration);

        services.AddHttpContextAccessor();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
        });

        services.AddPrincipalProvider();

        AddAuthentication(services);

        services.AddAuthorization(options =>
        {
            options.AddPolicies();
        });

        services.AddAuthorisationHandlers();

        services.AddScoped(provider => provider.GetRequiredService<IUserDataProvider>().GetCurrentUser());

        services.AddRepositories();
        services.AddEntities();
        services.AddServices();
        services.AddCommands();
        services.AddQueries();
        services.AddUserDataProvider();
        services.AddImporterFactory();

        services.AddIng();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStandardExceptionHandler();

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("User", httpContext.GetUserName());
            };
        });

        app.UseAuthentication();
        app.UseSerilogEnrichWithUser();
        app.UseStaticFiles();
        app.UseSpaStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = SpaRoot;

            spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    // never cache index.html
                    if (context.File.Name == "index.html")
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                }
            };

            if (env.IsDevelopment())
            {
                spa.UseProxyToSpaDevelopmentServer("http://localhost:3005");
            }
        });
    }

    private void AddAuthentication(IServiceCollection services)

    {
        //TODO added on token validated support in library.
        IdentityModelEventSource.ShowPII = true;
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

                        var claims = user.Accounts.Select(a => new Claim(Security.ClaimTypes.AccountId, a.AccountId.ToString())).ToList();

                        if (user.PrimaryAccountId != null) claims.Add(new(Security.ClaimTypes.PrimaryAccountId, user.PrimaryAccountId.Value.ToString()));
                        claims.Add(new(Security.ClaimTypes.FamilyId, user.FamilyId.ToString()));

                        principal.AddIdentity(new(claims));
                    }
                }
            };

            Configuration.Bind("OAuth", options.AzureOAuthOptions);
        });
    }
}
