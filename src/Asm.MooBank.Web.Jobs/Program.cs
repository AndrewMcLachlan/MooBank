using Asm.MooBank.Models;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;

return await WebJobStart.RunAsync(args, "Asm.MooBank.Web.Jobs", ConfigureWebJobs, ConfigureServices);

static void ConfigureWebJobs(IWebJobsBuilder builder)
{
    builder.AddTimers();
}

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddMooBankDbContext(context.HostingEnvironment, context.Configuration);
    services.AddRepositories();
    services.AddEntities();
    services.AddImporterFactory();
    services.AddServices();
    services.AddSingleton(new User() { EmailAddress = "moobank@mclachlan.family", Currency = String.Empty });

    services.AddSingleton<IAuthorizationService, AuthorisationService>();
    services.AddSingleton<IHttpContextAccessor, DummyHttpContextAccessor>();
    services.AddSingleton<IPrincipalProvider, PrincipalProvider>();

    services.AddIntegrations(context.Configuration);
}
