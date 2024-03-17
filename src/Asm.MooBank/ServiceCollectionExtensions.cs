using Asm.MooBank.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
    services.AddScoped<IRecurringTransactionService, RecurringTransactionService>()
            .AddScoped<ICurrencyConverter, CurrencyConverter>()
            .AddHostedService<PrecacheService>()
            .AddHostedService<RunRulesService>()
            .AddSingleton<IRunRulesQueue, RunRulesQueue>()
            .AddLazyCache();
}
