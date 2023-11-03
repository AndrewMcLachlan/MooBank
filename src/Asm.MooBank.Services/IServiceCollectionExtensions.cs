using Asm.MooBank.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class AsmMooBankServicesIServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services.AddScoped<IAccountHolderService, AccountHolderService>()
                .AddScoped<IImportService, ImportService>()
                .AddScoped<IRecurringTransactionService, RecurringTransactionService>()
                //.AddScoped<IReferenceDataService, ReferenceDataService>()
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped<ITransactionTagService, TransactionTagService>()
                .AddHostedService<RunRulesService>()
                .AddSingleton<IRunRulesQueue, RunRulesQueue>();
}
