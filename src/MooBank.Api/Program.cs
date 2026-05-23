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
using Asm.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore.Authentication;

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
        new Asm.MooBank.Modules.Forecast.Module(),
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
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
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

        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.JsonTypeInfo.Kind != System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
                return Task.CompletedTask;

            var nullabilityContext = new NullabilityInfoContext();

            foreach (var jsonProperty in context.JsonTypeInfo.Properties)
            {
                if (jsonProperty.AttributeProvider is not PropertyInfo propertyInfo)
                    continue;

                var nullabilityInfo = nullabilityContext.Create(propertyInfo);

                if (nullabilityInfo.WriteState != NullabilityState.Nullable)
                {
                    schema.Required ??= new HashSet<string>();
                    schema.Required.Add(jsonProperty.Name);
                }
            }

            return Task.CompletedTask;
        });

        options.CreateSchemaReferenceId = arg =>
        {
            if (arg.Kind == System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
            {
                return arg.Type.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName ?? OpenApiOptions.CreateDefaultSchemaReferenceId(arg);
            }
            return OpenApiOptions.CreateDefaultSchemaReferenceId(arg);
        };
    });

    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddProblemDetailsFactory();

    services.AddMooBankDbContext(builder.Environment, builder.Configuration);

    services.AddHttpContextAccessor();

    services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });

    services.AddPrincipalProvider();

    AzureOAuthOptions oAuthOptions = builder.Configuration.GetSection("OAuth").Get<AzureOAuthOptions>() ?? throw new InvalidOperationException("OAuth config not defined");

    services.AddAuthentication(builder.Configuration)
        .AddMcp(options =>
        {
            // Validate tokens via the existing JwtBearer pipeline; the MCP scheme
            // keeps ownership of the 401 challenge so it can emit the
            // resource_metadata pointer the MCP spec requires.
            options.ForwardAuthenticate = JwtBearerDefaults.AuthenticationScheme;
            options.ForwardForbid = JwtBearerDefaults.AuthenticationScheme;
            // The MCP spec / Claude Desktop's Custom Connector require the `resource`
            // field in the Protected Resource Metadata document to match the MCP
            // server URL exactly as the user enters it in the client, INCLUDING the
            // path component. Using the App ID URI ("api://...") here makes Claude
            // reject the metadata document and fall back to treating MooBank as the
            // authorization server. Token audience validation is unaffected because
            // it's driven by JwtBearer config, not this value.
            //
            // Entra v2's /authorize endpoint also requires the `scope` parameter and
            // the `resource` parameter to point at the same App ID URI, or it
            // rejects with AADSTS9010010 ("resource parameter doesn't match requested
            // scopes"). The scope advertised here must therefore live under the same
            // App ID URI as the Resource value above — i.e. an Entra app whose App
            // ID URI is https://moobank.mclachlan.family/mcp, with `api.read`
            // exposed under it.
            options.ResourceMetadata = new()
            {
                Resource = "https://moobank.mclachlan.family/mcp",
                AuthorizationServers = { oAuthOptions.Authority },
                ScopesSupported = ["https://moobank.mclachlan.family/mcp/api.read"],
            };
        });

    // The SPA's MSAL config requests tokens for api://moobank.mclachlan.family
    // (the original resource app's App ID URI). The MCP connector's tokens come
    // from a separate Entra app with App ID URI https://moobank.mclachlan.family/mcp.
    // Accept both audiences (with and without trailing slash, since Anthropic's
    // OAuth client has a known habit of normalising URIs with a trailing slash)
    // on the JwtBearer pipeline so a single MooBank instance serves both surfaces
    // without splitting the API.
    services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var existing = options.TokenValidationParameters.ValidAudiences?.ToList() ?? [];
        if (options.TokenValidationParameters.ValidAudience is { Length: > 0 } single)
        {
            existing.Add(single);
        }
        existing.Add("api://moobank.mclachlan.family");
        existing.Add("https://moobank.mclachlan.family/mcp");
        existing.Add("https://moobank.mclachlan.family/mcp/");
        options.TokenValidationParameters.ValidAudiences = existing.Distinct().ToList();

        // Diagnostic: log the exact reason when token validation fails. Remove once
        // MCP authn is confirmed working end-to-end.
        var existingFail = options.Events?.OnAuthenticationFailed;
        options.Events ??= new();
        options.Events.OnAuthenticationFailed = async ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("MooBank.JwtBearer");
            logger.LogWarning(ctx.Exception,
                "JwtBearer auth failed on {Path}. Accepted audiences: [{Audiences}]. Exception type: {Type}",
                ctx.HttpContext.Request.Path,
                string.Join(", ", ctx.Options.TokenValidationParameters.ValidAudiences ?? []),
                ctx.Exception?.GetType().FullName);
            if (existingFail != null) await existingFail(ctx);
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

    services.AddIntegrations(builder.Configuration);

    var headerPolicies = services.AddStandardSecurityHeaders()
        .AddContentSecurityPolicy(options =>
        {
            options.AddDefaultSrc().Self();
            options.AddConnectSrc().Self().From("https://login.microsoftonline.com").From("https://graph.microsoft.com");
            options.AddFrameSrc().Self().From("https://login.microsoftonline.com");
            options.AddFormAction().Self().From("https://login.microsoftonline.com");
            options.AddImgSrc().Self().Data().Blob().From("https://cdn.mclachlan.family");
            options.AddFontSrc().Self().From("https://cdn.mclachlan.family");
            options.AddStyleSrc().Self().UnsafeInline();
            options.AddScriptSrc().Self().UnsafeInline();
        })
        .AddPermissionsPolicyWithDefaultSecureDirectives();

    // MSAL loads Microsoft's authorize endpoint in a hidden iframe and that response
    // does not include Cross-Origin-Resource-Policy. With COEP=require-corp the browser
    // blocks it. We don't use SharedArrayBuffer or any other COEP-gated feature, so
    // remove the header entirely rather than relying on UnsafeNone overriding the default.
    headerPolicies.Remove("Cross-Origin-Embedder-Policy");

    // Register WebJobs SDK for in-process background jobs
    builder.Host.ConfigureWebJobs(webJobsBuilder =>
    {
        webJobsBuilder.AddTimers();
    });
}

void AddApp(WebApplication app)
{
    // MooBank is always served over HTTPS in production but the App Service /
    // Cloudflare hop terminates TLS, so the app sees the inbound scheme as http.
    // Force it back to https so URL generation (e.g. the resource_metadata URL in
    // the MCP WWW-Authenticate header) emits the correct scheme. Skipped in dev
    // where the SPA proxy may use http://localhost.
    if (!app.Environment.IsDevelopment())
    {
        app.Use((ctx, next) =>
        {
            ctx.Request.Scheme = "https";
            return next();
        });
    }

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
        });
    }

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

    app.UseAuthentication();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseAuthorization();

    app.MapMcp("mcp").RequireAuthorization(new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(McpAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
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

