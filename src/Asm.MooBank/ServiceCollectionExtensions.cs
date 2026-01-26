using Asm.MooBank.Queues;
using Asm.MooBank.Services;
using Asm.MooBank.Services.Background;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
    services.AddScoped<IRecurringTransactionService, RecurringTransactionService>()
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
            .AddSingleton<IImportTransactionsQueue, ImportTransactionsQueue>()
            .AddLazyCache();


    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration) =>
            services.AddEodhd(options => configuration.Bind("EODHD", options))
                    .AddExchangeRateApi(options => configuration.Bind("ExchangeRateApi", options))
                    .AddAbs()
                    .AddScoped<IStockPriceService, StockPriceService>()
                    .AddScoped<IExchangeRateService, ExchangeRateService>()
                    .AddScoped<ICpiChangeService, CpiChangeService>();
}
