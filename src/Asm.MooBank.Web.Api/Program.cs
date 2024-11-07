using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Institution.AustralianSuper;
using Asm.MooBank.Institution.Ing;
using Asm.MooBank.Security;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;

return WebApplicationStart.Run(args, "Asm.MooBank.Web.Api", AddServices, AddApp);

void AddServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    builder.RegisterModules(() =>
        new IModule[]
        {
            new Asm.MooBank.Modules.Accounts.Module(),
            new Asm.MooBank.Modules.Assets.Module(),
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
        });

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

        options.SwaggerDoc("v1", new()
        {
            Title = "MooBank API",
            Version = fileVersionInfo.FileVersion
        });

        options.CustomSchemaIds(type => type.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName ?? type.Name);

        options.AddSecurityDefinition("oidc", new()
        {
            Type = SecuritySchemeType.OpenIdConnect,
            OpenIdConnectUrl = new Uri($"{builder.Configuration["OAuth:Domain"]}{builder.Configuration["OAuth:TenantId"]}/v2.0/.well-known/openid-configuration"),
        });

        options.AddSecurityRequirement(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oidc" }
                },[]
            },
        });
    });

    if (builder.Environment.IsProduction())
    {
        services.AddOpenTelemetry().WithMetrics().UseAzureMonitor();
    }

    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

    services.AddProblemDetailsFactory();

    services.AddMooBankDbContext(builder.Environment, builder.Configuration);
    services.AddCacheableData();

    services.AddHttpContextAccessor();
    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.AddHealthChecks();
}

void AddApp(WebApplication app)
{
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(app.Configuration["OAuth:Audience"]);
        options.OAuthAppName("MooBank");
        options.OAuthUsePkce();
        options.OAuthScopes("api://moobank.mclachlan.family/.default");
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
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseAuthorization();

    IEndpointRouteBuilder builder = app.MapGroup("/api")
        .WithOpenApi();

    builder.MapModuleEndpoints();

    app.UseSecurityHeaders();

    app.MapFallbackToFile("/index.html");

}
