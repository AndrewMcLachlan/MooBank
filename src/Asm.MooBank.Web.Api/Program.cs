using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Asm.AspNetCore.Api;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.AustralianSuper;
using Asm.MooBank.Institution.Ing;
using Asm.MooBank.Institution.Macquarie;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi;

var result = WebApplicationStart.Run(args, "Asm.MooBank.Web.Api", AddServices, AddApp, AddHealthChecks);

return result;

void AddServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    builder.RegisterModules(() =>
    [
        new Asm.MooBank.Modules.Accounts.Module(),
        new Asm.MooBank.Modules.Assets.Module(),
        new Asm.MooBank.Modules.Bills.Module(),
        new Asm.MooBank.Modules.Budgets.Module(),
        new Asm.MooBank.Modules.Families.Module(),
        new Asm.MooBank.Modules.Groups.Module(),
        new Asm.MooBank.Modules.Institutions.Module(),
        new Asm.MooBank.Modules.Instruments.Module(),
        new Asm.MooBank.Modules.ReferenceData.Module(),
        new Asm.MooBank.Modules.Reports.Module(),
        new Asm.MooBank.Modules.Stocks.Module(),
        new Asm.MooBank.Modules.Tags.Module(),
        new Asm.MooBank.Modules.Transactions.Module(),
        new Asm.MooBank.Modules.Users.Module(),
    ]);

    services.AddEndpointsApiExplorer();
    services.AddAzureOAuthOptions("OAuth");

    services.AddOpenApi("v1", options =>
    {
        options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        options.AddDocumentTransformer<OidcSecuritySchemeTransformer>();
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            document.Info.Title = "MooBank API";
            document.Info.Version = fileVersionInfo.FileVersion;

            document.Tags ??= new HashSet<OpenApiTag>();
            document.Tags.AddRange(document.Tags.OrderBy(t => t.Name));

            return Task.CompletedTask;
        });

        options.CreateSchemaReferenceId = arg =>
        {
            if (arg.Kind == System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
            {
                return arg.Type.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName ?? arg.Type.Name;
            }
            return OpenApiOptions.CreateDefaultSchemaReferenceId(arg);
        };

        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            return Task.CompletedTask;
        });
    });

    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

    services.AddProblemDetailsFactory();

    services.AddMooBankDbContext(builder.Environment, builder.Configuration);
    services.AddCacheableData();

    services.AddHttpContextAccessor();

    services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });

    services.AddPrincipalProvider();

    services.AddAuthentication(builder.Configuration);

    services.AddAuthorization(options =>
    {
        options.AddPolicies();
    });

    services.AddAuthorisationHandlers();

    services.AddScoped(provider => provider.GetRequiredService<IUserDataProvider>().GetCurrentUser());

    services.AddRepositories();
    services.AddEntities();
    services.AddServices();
    services.AddUserDataProvider();
    services.AddImporterFactory();

    services.AddIng();
    services.AddAustralianSuper();
    services.AddMacquarie();

    services.AddHealthChecks();

    // Register WebJobs SDK for in-process background jobs
    builder.Host.ConfigureWebJobs(webJobsBuilder =>
    {
        webJobsBuilder.AddTimers();
    });
    
    // Ensure WebJobs host has access to all services
    builder.Host.ConfigureServices((context, webJobServices) =>
    {
        // Register all necessary services for WebJobs
        webJobServices.AddMooBankDbContext(builder.Environment, builder.Configuration);
        webJobServices.AddRepositories();
        webJobServices.AddEntities();
        webJobServices.AddServices();
        webJobServices.AddIntegrations(context.Configuration);
    });
}

void AddApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "MooBank API");
            options.OAuthClientId(app.Configuration["OAuth:Audience"]);
            options.OAuthAppName("MooBank");
            options.OAuthUsePkce();
            options.OAuthScopes("api://moobank.mclachlan.family/.default");

            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MooBank API (Swashbuckle)");
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
    }

    app.UseStandardExceptionHandler();

    app.UseAuthentication();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseAuthorization();

    IEndpointRouteBuilder builder = app.MapGroup("/api");

    builder.MapModuleEndpoints();

    app.UseSecurityHeaders();

    app.MapFallbackToFile("/index.html");
}

void AddHealthChecks(IHealthChecksBuilder builder, WebApplicationBuilder app)
{
    builder.AddDbContextCheck<MooBankContext>("MooBankDbContext", tags: ["health", "db"]);
}

