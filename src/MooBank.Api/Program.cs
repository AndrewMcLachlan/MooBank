using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Asm.AspNetCore.Api;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Api.Middleware;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.AustralianSuper;
using Asm.MooBank.Institution.Ing;
using Asm.MooBank.Institution.Macquarie;
using Asm.MooBank.Security;
using Asm.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore.Authentication;

var result = WebApplicationStart.Run(args, "Asm.MooBank.Api", AddServices, AddApp, AddHealthChecks);

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
        new Asm.MooBank.Modules.Forecast.Module(),
        new Asm.MooBank.Modules.Groups.Module(),
        new Asm.MooBank.Modules.Institutions.Module(),
        new Asm.MooBank.Modules.Instruments.Module(),
        new Asm.MooBank.Modules.ReferenceData.Module(),
        new Asm.MooBank.Modules.Reports.Module(),
        new Asm.MooBank.Modules.Retirement.Module(),
        new Asm.MooBank.Modules.Stocks.Module(),
        new Asm.MooBank.Modules.Tags.Module(),
        new Asm.MooBank.Modules.Transactions.Module(),
        new Asm.MooBank.Modules.Users.Module(),
    ]);

    services.AddPostieEndpointDispatcher();

    services.AddEndpointsApiExplorer();
    services.AddAzureOAuthOptions("OAuth");

    services.AddOpenApi("v1", options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        options.AddDocumentTransformer<OidcSecuritySchemeTransformer>();
        options.AddDocumentInfo("MooBank API", Assembly.GetExecutingAssembly());
        options.RelocatePathPrefixToServer("/api");
        options.AddRequiredForNonNullableProperties();
        options.UseDisplayNameSchemaReferenceIds();
    });

    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddAsmExceptionHandler();

    services.AddMooBankDbContext(builder.Environment, builder.Configuration);

    services.AddHttpContextAccessor();

    services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });

    services.AddPrincipalProvider();

    AzureOAuthOptions oAuthOptions = builder.Configuration.GetSection("OAuth").Get<AzureOAuthOptions>() ?? throw new InvalidOperationException("OAuth config not defined");

    // The URL clients reach the MCP endpoint on. It is published as the `resource` of the Protected
    // Resource Metadata document, and the client sends that value straight back to the authorisation
    // server as the RFC 8707 resource indicator. Both ends match it exactly: the client requires the
    // same origin as the URL it connected to, and Entra requires a registered App ID URI, scheme,
    // port and path included. Anything else fails the handshake before a token is ever issued.
    string mcpResource = builder.Configuration["Mcp:Resource"] ?? throw new InvalidOperationException("Mcp:Resource config not defined");

    services.AddAuthentication(builder.Configuration)
        .AddMcp(options =>
        {
            // Validate tokens via the existing JwtBearer pipeline; the MCP scheme
            // keeps ownership of the 401 challenge so it can emit the
            // resource_metadata pointer the MCP spec requires.
            options.ForwardAuthenticate = JwtBearerDefaults.AuthenticationScheme;
            options.ForwardForbid = JwtBearerDefaults.AuthenticationScheme;
            options.ResourceMetadata = new()
            {
                Resource = mcpResource,
                AuthorizationServers = { oAuthOptions.Authority },
                // Entra addresses a delegated scope as "<App ID URI>/<scope>" and rejects an
                // authorize request whose scope and resource sit under different App ID URIs.
                ScopesSupported = [$"{mcpResource}/api.read"],
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicies();
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
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

    services.AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "MooBank",
            Version = "0.1",
            Icons =
            [
                new()
                {
                    MimeType = "image/svg+xml",
                    Source = "https://cdn.mclachlan.family/images/moo/logo.svg",
                }
            ],
        };
    })
        .WithHttpTransport()
        .WithToolsFromAssemblies("Asm.MooBank.Modules");

    services.AddEodhd(options => builder.Configuration.Bind("EODHD", options))
            .AddExchangeRateApi(options => builder.Configuration.Bind("ExchangeRateApi", options))
            .AddAbs()
            .AddIntegrationServices();

    services.AddStandardSecurityHeaders(policies =>
    {
        policies.AddContentSecurityPolicy(options =>
        {
            options.AddDefaultSrc().Self();
            options.AddConnectSrc().Self().From("https://login.microsoftonline.com").From("https://graph.microsoft.com");
            options.AddFrameSrc().Self().From("https://login.microsoftonline.com");
            options.AddFormAction().Self().From("https://login.microsoftonline.com");
            options.AddImgSrc().Self().Data().Blob().From("https://cdn.mclachlan.family");
            options.AddFontSrc().Self().From("https://cdn.mclachlan.family");
            options.AddStyleSrc().Self().UnsafeInline();
            options.AddScriptSrc().Self().UnsafeInline();
        });

        policies.AddPermissionsPolicyWithDefaultSecureDirectives();

        // MSAL loads Microsoft's authorize endpoint in a hidden iframe and that response
        // does not include Cross-Origin-Resource-Policy. With COEP=require-corp the browser
        // blocks it. We don't use SharedArrayBuffer or any other COEP-gated feature, so
        // remove the header entirely rather than relying on UnsafeNone overriding the default.
        policies.Remove("Cross-Origin-Embedder-Policy");
    });

    // Register WebJobs SDK for in-process background jobs
    builder.Host.ConfigureWebJobs(webJobsBuilder =>
    {
        webJobsBuilder.AddTimers();
    });
}

void AddApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi("api/openapi/{documentName}.json").AllowAnonymous();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api/swagger";
            options.SwaggerEndpoint("/api/openapi/v1.json", "MooBank API");
            options.OAuthClientId(app.Configuration["OAuth:Audience"]);
            options.OAuthAppName("MooBank");
            options.OAuthUsePkce();
            options.OAuthScopes("api://moobank.mclachlan.family/.default");
        });
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
        app.UseHttpsRedirection();

        // MooBank is always served over HTTPS in production but the App Service /
        // Cloudflare hop terminates TLS, so the app sees the inbound scheme as http.
        // Force it back to https so URL generation (e.g. the resource_metadata URL in
        // the MCP WWW-Authenticate header) emits the correct scheme. Skipped in dev
        // where the SPA proxy may use http://localhost.
        app.Use((ctx, next) =>
        {
            ctx.Request.Scheme = "https";
            return next();
        });
    }

    app.UseStandardExceptionHandler();

    app.UseAuthentication();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseMiddleware<AuditMiddleware>();

    app.UseAuthorization();

    app.MapMcp("mcp").RequireAuthorization(new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(McpAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        // The resource metadata advertises api.read; require it rather than accepting any token
        // that is merely valid for the API audience.
        .RequireAssertion(context => context.User
            .FindAll(c => c.Type == "scp" || c.Type == "http://schemas.microsoft.com/identity/claims/scope")
            .SelectMany(c => c.Value.Split(' '))
            .Contains("api.read"))
        .Build());

    IEndpointRouteBuilder builder = app.MapGroup("/api");

    builder.MapModuleEndpoints();

    app.UseStandardSecurityHeaders();

    app.MapFallbackToFile("/index.html").AllowAnonymous();
}

void AddHealthChecks(IHealthChecksBuilder builder, WebApplicationBuilder app)
{
    builder.AddDbContextCheck<MooBankContext>("MooBankDbContext", tags: ["health", "db"]);
}

