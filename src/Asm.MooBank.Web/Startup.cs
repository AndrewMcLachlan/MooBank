using System.Net;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Repositories;
using Asm.MooBank.Domain.Repositories.Ing;
using Asm.MooBank.Importers;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Repositories.Ing;
using Asm.MooBank.Security;
using Asm.MooBank.Services.Importers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace Asm.MooBank.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddControllers();

        services.AddProblemDetailsFactory();

        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "MookBankApp/build";
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

        services.AddAuthorization();

        services.AddRepositories();
        services.AddServices();
        services.AddUserDataProvider();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
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
            spa.Options.SourcePath = "MookBankApp";

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
        IdentityModelEventSource.ShowPII = true;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddAzureAdBearer(options => Configuration.Bind("OAuth", options));
    }
}
