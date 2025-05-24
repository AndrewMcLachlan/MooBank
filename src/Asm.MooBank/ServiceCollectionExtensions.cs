using Asm.MooBank.Services;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
    services.AddScoped<IRecurringTransactionService, RecurringTransactionService>()
            .AddScoped<ICurrencyConverter, CurrencyConverter>()
            .AddScoped<ICpiService, CpiService>()
            .AddHostedService<PrecacheService>()
            .AddHostedService<RunRulesService>()
            .AddHostedService<ReprocessTransactionsService>()
            .AddSingleton<IRunRulesQueue, RunRulesQueue>()
            .AddSingleton<IReprocessTransactionsQueue, ReprocessTransactionsQueue>()
            .AddLazyCache();


    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration) =>
            services.AddEodhd(options => configuration.Bind("EODHD", options))
                    .AddExchangeRateApi(options => configuration.Bind("ExchangeRateApi", options))
                    .AddAbs()
                    .AddScoped<IStockPriceService, StockPriceService>()
                    .AddScoped<IExchangeRateService, ExchangeRateService>()
                    .AddScoped<ICpiChangeService, CpiChangeService>();
}
