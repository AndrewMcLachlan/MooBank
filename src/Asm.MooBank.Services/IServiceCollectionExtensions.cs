using Asm.Cqrs.Queries;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Asm.MooBank.Services.Importers;
using Asm.MooBank.Services.Queries;

namespace Microsoft.Extensions.DependencyInjection;

public static class AsmMooBankServicesIServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services.AddScoped<IAccountService, AccountService>()
                .AddScoped<IAccountHolderService, AccountHolderService>()
                .AddScoped<IImportService, ImportService>()
                .AddScoped<IRecurringTransactionService, RecurringTransactionService>()
                .AddScoped<IReferenceDataService, ReferenceDataService>()
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped<ITransactionTagService, TransactionTagService>()
                .AddScoped<ITransactionTagRuleService, TransactionTagRuleService>()
                .AddScoped<IVirtualAccountService, VirtualAccountService>()

                .AddHostedService<RunRulesService>()
                .AddSingleton<IRunRulesQueue, RunRulesQueue>()
                .AddScoped<IImporterFactory, ImporterFactory>()

                .AddImporters();

    public static IServiceCollection AddImporters(this IServiceCollection services) =>
        services.AddScoped<IngImporter>();

    public static IServiceCollection AddCqrs(this IServiceCollection services) =>
        services.AddCommandHandlers(typeof(AsmMooBankServicesIServiceCollectionExtensions).Assembly)
                //.AddSingleton<Asm.Cqrs.Queries.IQueryDispatcher, VanillaQueryDispatcher>()
                //.AddTransient<IQueryDispatcher2, VanillaQueryDispatcher>()
                //.AddTransient<IQueryHandler<Asm.MooBank.Models.Queries.GetAccountGroups, IEnumerable<AccountGroup>>, GetAccountGroups>();
                //.AddTransient<IQueryHandler<IQuery<IEnumerable<AccountGroup>>, IEnumerable<AccountGroup>>, GetAccountGroups>();
    .AddQueryHandlers(typeof(AsmMooBankServicesIServiceCollectionExtensions).Assembly);
}
