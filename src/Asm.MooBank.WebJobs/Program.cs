using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;

return await WebJobStart.RunAsync(args, "MooBank", ConfigureServices, Runner);

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddMooBankDbContext(context.Configuration);
    services.AddRepositories();
    services.AddServices();
    services.AddSingleton<IUserDataProvider, UserDataProvider>();
    services.AddSingleton<IHttpContextAccessor, DummyHttpContextAccessor>();
}

static async Task Runner(IHost host)
{
    using var scope = host.Services.CreateScope();
    var recurringTransactionService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();

    await recurringTransactionService.Process();
}