using Asm.MooBank.Audit;
using Asm.MooBank.Queues;
using Asm.MooBank.Services;
using Asm.MooBank.Services.Background;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogger, AuditLogger>()
            .AddScoped<IAuditingUnitOfWork, AuditingUnitOfWork>()
            .AddScoped<IRecurringTransactionService, RecurringTransactionService>()
            .AddScoped<ICurrencyConverter, CurrencyConverter>()
            .AddScoped<ICpiService, CpiService>()
            .AddScoped<IRunRulesService, RunRulesService>()
            .AddScoped<IReprocessTransactionsService, ReprocessTransactionsService>()
            .AddScoped<IImportTransactionsService, ImportTransactionsService>()
            .AddHostedService<PrecacheService>()
            .AddHostedService<RunRulesBackgroundService>()
            .AddHostedService<ReprocessTransactionsBackgroundService>()
            .AddHostedService<ImportTransactionsBackgroundService>()
            .AddSingleton<IRunRulesQueue, RunRulesQueue>()
            .AddSingleton<IReprocessTransactionsQueue, ReprocessTransactionsQueue>()
            .AddSingleton<IImportTransactionsQueue, ImportTransactionsQueue>();

        services.AddHybridCache();

        return services;
    }


    public static IServiceCollection AddIntegrationServices(this IServiceCollection services) =>
            services.AddScoped<IStockPriceService, StockPriceService>()
                    .AddScoped<IExchangeRateService, ExchangeRateService>()
                    .AddScoped<ICpiChangeService, CpiChangeService>();
}
