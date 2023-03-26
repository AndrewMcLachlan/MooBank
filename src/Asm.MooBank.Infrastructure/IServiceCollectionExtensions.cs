using System.Reflection;
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Repositories.Ing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    //private static readonly Type IQueryableType = typeof(IQueryable<>);
    //private static readonly MethodInfo DbContextSetMethod = typeof(DbContext).GetTypeInfo().GetDeclaredMethods("Set").Single(mi => !mi.GetParameters().Any())!;
    //private static readonly MethodInfo DbContextSetMethod = typeof(IReadOnlyDbContext).GetTypeInfo().GetDeclaredMethod(nameof(IReadOnlyDbContext.Set))!;
    //private static readonly MethodInfo AsNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!;

    public static IServiceCollection AddMooBankDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankPlusContext>((services, options) => options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.EnableRetryOnFailure(3);
        }));

        //HACK: To be fixed
        services.AddReadOnlyDbContext<IReadOnlyDbContext, BankPlusContext>((services, options) => options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.EnableRetryOnFailure(3);
        }));


        return services.AddUnitOfWork<BankPlusContext>();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IInstitutionAccountRepository, InstitutionAccountRepository>()
                .AddScoped<IAccountGroupRepository, AccountGroupRepository>()
                .AddScoped<IAccountHolderRepository, AccountHolderRepository>()
                .AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>()
                .AddScoped<IReferenceDataRepository, ReferenceDataRepository>()
                .AddScoped<ISecurityRepository, SecurityRepository>()
                .AddScoped<ITransactionExtraRepository, TransactionExtraRepository>()
                .AddScoped<ITransactionRepository, TransactionRepository>()
                .AddScoped<ITransactionTagRepository, TransactionTagRepository>()
                .AddScoped<ITransactionTagRuleRepository, TransactionTagRuleRepository>()
                .AddScoped<IVirtualAccountRepository, VirtualAccountRepository>();

    public static IServiceCollection AddEntities(this IServiceCollection services) =>
        services
        //.AddQueryable2<AccountGroup, BankPlusContext>()
                .AddAggregateRoots<BankPlusContext>(typeof(IAccountGroupRepository).Assembly);

    /*public static IServiceCollection AddAggregateRoots<TContext>(this IServiceCollection services, Assembly entityAssembly) where TContext : IReadOnlyDbContext
    {
        var types = entityAssembly.GetTypes().Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(AggregateRootAttribute)));

        foreach (var type in types)
        {
            var queryableGeneric = IQueryableType.MakeGenericType(type);

            services.Add(new ServiceDescriptor(queryableGeneric, sp =>
            {
                IReadOnlyDbContext context = sp.GetRequiredService<TContext>();

                var genericSet = DbContextSetMethod.MakeGenericMethod(type);

                var res = genericSet.Invoke(context, null);

                var genericAsNoTracking = AsNoTrackingMethod.MakeGenericMethod(type);

                var res2 = genericAsNoTracking.Invoke(null, new[] { res });

                return res2!;
            }, ServiceLifetime.Transient));
        }
        return services;
    }


    public static IServiceCollection AddQueryable2<TEntity, TContext>(this IServiceCollection services) where TEntity : class where TContext : IReadOnlyDbContext =>
    services.AddTransient(sp =>
    {
        var c = sp.GetRequiredService<TContext>().Set<TEntity>().AsNoTracking();
        return c;
    });*/
}
