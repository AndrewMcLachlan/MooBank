﻿using System.Reflection;
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Security;
using LazyCache;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddMooBankDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MooBankContext>((services, options) => options.UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.UseAzureSqlDefaults();
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));

        //HACK: To be fixed
        services.AddReadOnlyDbContext<IReadOnlyDbContext, MooBankContext>((services, options) => options.UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.UseAzureSqlDefaults();
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));

        services.AddDomainEvents(typeof(Instrument).Assembly);

        return services.AddUnitOfWork<MooBankContext>();
    }

    // Keeping this here in case this code becomes useful in the future.
    // Provides a cache of data.
    public static IServiceCollection AddCacheableData(this IServiceCollection services) => services;
    /*services.AddScoped<IEnumerable<TagRelationship>>(provider =>
    {
        var cache = provider.GetRequiredService<IAppCache>();

        return cache.GetOrAdd<IEnumerable<TagRelationship>>("tag-relationships", () =>
        {
            var context = provider.GetRequiredService<IReadOnlyDbContext>();

            return context.Set<TagRelationship>().ToList();
        }, new Caching.Memory.MemoryCacheEntryOptions
        {
            Priority = Caching.Memory.CacheItemPriority.NeverRemove
        });
    });*/

    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IInstitutionAccountRepository, InstitutionAccountRepository>()
                .AddScoped<IInstrumentRepository, InstrumentRepository>()
                .AddScoped<IGroupRepository, GroupRepository>()
                .AddScoped<IAccountHolderRepository, AccountHolderRepository>()
                .AddScoped<IAssetRepository, AssetRepository>()
                .AddScoped<IBudgetRepository, BudgetRepository>()
                .AddScoped<IFamilyRepository, FamilyRepository>()
                .AddScoped<IInstitutionRepository, InstitutionRepository>()
                .AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>()
                .AddScoped<IReferenceDataRepository, ReferenceDataRepository>()
                .AddScoped<ISecurity, SecurityRepository>()
                .AddScoped<IStockHoldingRepository, StockHoldingRepository>()
                .AddScoped<ITransactionRepository, TransactionRepository>()
                .AddScoped<ITagRepository, TransactionTagRepository>()
                .AddScoped<IRuleRepository, RuleRepository>();

    public static IServiceCollection AddEntities(this IServiceCollection services) =>
        services.AddAggregateRoots<MooBankContext>(typeof(IGroupRepository).Assembly);

    public static IServiceCollection AddImporterFactory(this IServiceCollection services) =>
        services.AddTransient<IImporterFactory, ImporterFactory>();
}
