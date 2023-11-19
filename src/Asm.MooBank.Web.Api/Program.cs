using System.Text.Json.Serialization;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Institution.Ing;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;

WebApplicationStart.Run(args, "Asm.MooBank.Web.Api", AddServices, AddApp);


void AddServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    builder.RegisterModules(() =>
        new IModule[]
        {
            new Asm.MooBank.Modules.Account.Module(),
            new Asm.MooBank.Modules.AccountGroup.Module(),
            new Asm.MooBank.Modules.Budget.Module(),
            new Asm.MooBank.Modules.Family.Module(),
            new Asm.MooBank.Modules.Institution.Module(),
            new Asm.MooBank.Modules.ReferenceData.Module(),
            new Asm.MooBank.Modules.Reports.Module(),
            new Asm.MooBank.Modules.Tag.Module(),
            new Asm.MooBank.Modules.Transactions.Module(),
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

        options.CustomSchemaIds(type => type.FullName?
            //.Replace("Asm.MooBank.Modules", String.Empty)
            //.Replace("Asm.MooBank.Models", String.Empty)
            .Replace('+', '.'));
    });

    services.AddApplicationInsightsTelemetry();

    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

    services.AddProblemDetailsFactory();
    //services.AddProblemDetails();

    services.AddMooBankDbContext(builder.Configuration);

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
    //services.AddCommands();
    //services.AddQueries();
    services.AddUserDataProvider();
    services.AddImporterFactory();

    services.AddIng();

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

void AddApp(WebApplication app)
{
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI();

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

    app.MapFallbackToFile("/index.html");

}
