using Asm.Hosting;
using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Asm.MooBank.Services.Background;
using Asm.MooBank.Services.DemoData;

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
            .AddScoped<IDemoDataService, DemoDataService>()
            .AddScoped<IDemoDataWriter, DemoDataWriter>()
            .AddHostedService<PrecacheService>()
            .AddHostedService<RunRulesBackgroundService>()
            .AddHostedService<ReprocessTransactionsBackgroundService>()
            .AddHostedService<ImportTransactionsBackgroundService>()
            .AddBackgroundWorkQueue<Guid>()
            .AddBackgroundWorkQueue<ReprocessWorkItem>()
            .AddBackgroundWorkQueue<ImportWorkItem>();

        // Bound whether or not the section exists: an absent section leaves every id null, which is
        // how the demo data job is switched off.
        services.AddOptions<DemoDataOptions>().BindConfiguration(DemoDataOptions.SectionName);

        services.AddHybridCache();

        return services;
    }


    public static IServiceCollection AddIntegrationServices(this IServiceCollection services) =>
            services.AddScoped<IStockPriceService, StockPriceService>()
                    .AddScoped<IExchangeRateService, ExchangeRateService>()
                    .AddScoped<ICpiChangeService, CpiChangeService>();
}
