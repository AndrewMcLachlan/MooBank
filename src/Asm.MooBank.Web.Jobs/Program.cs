using Asm.MooBank.Models;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;

return await WebJobStart.RunAsync(args, "MooBank", ConfigureWebJobs, ConfigureServices);

static void ConfigureWebJobs(IWebJobsBuilder builder)
{
    builder.AddTimers();
}

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddMooBankDbContext(context.Configuration);
    services.AddRepositories();
    services.AddEntities();
    services.AddImporterFactory();
    services.AddServices();
    services.AddSingleton(new AccountHolder() { EmailAddress = "moobank@mclachlan.family"});

    services.AddSingleton<IAuthorizationService, AuthorisationService>();
    services.AddSingleton<IUserDataProvider, UserDataProvider>();
    services.AddSingleton<IHttpContextAccessor, DummyHttpContextAccessor>();
    services.AddSingleton<IPrincipalProvider, PrincipalProvider>();
}