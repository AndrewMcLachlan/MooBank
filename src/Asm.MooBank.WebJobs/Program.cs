using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

return await WebJobStart.RunAsync(args, "MooBank", ConfigureServices, Runner);

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddMooBankDbContext(context.Configuration);
    services.AddRepositories();
    services.AddEntities();
    services.AddImporterFactory();
    services.AddServices();
    services.AddSingleton(new AccountHolder());

    services.AddSingleton<IAuthorizationService, AuthorisationService>();
    services.AddSingleton<IUserDataProvider, UserDataProvider>();
    services.AddSingleton<IHttpContextAccessor, DummyHttpContextAccessor>();
    services.AddSingleton<IPrincipalProvider, PrincipalProvider>();
}

static async Task Runner(IHost host)
{
    using var scope = host.Services.CreateScope();
    var recurringTransactionService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();

    await recurringTransactionService.Process();
}