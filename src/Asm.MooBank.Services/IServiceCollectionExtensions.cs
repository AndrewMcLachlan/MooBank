using Asm.MooBank.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class AsmMooBankServicesIServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services.AddScoped<IImportService, ImportService>()
                .AddScoped<IRecurringTransactionService, RecurringTransactionService>()
                .AddHostedService<RunRulesService>()
                .AddSingleton<IRunRulesQueue, RunRulesQueue>();
}
