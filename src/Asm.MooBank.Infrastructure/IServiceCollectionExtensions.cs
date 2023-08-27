using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Repositories.Ing;
using Asm.MooBank.Security;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddMooBankDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MooBankContext>((services, options) => options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.EnableRetryOnFailure(3);
            options.UseDateOnlyTimeOnly();
        }));

        //HACK: To be fixed
        services.AddReadOnlyDbContext<IReadOnlyDbContext, MooBankContext>((services, options) => options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.EnableRetryOnFailure(3);
            options.UseDateOnlyTimeOnly();
        }));


        return services.AddUnitOfWork<MooBankContext>();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IInstitutionAccountRepository, InstitutionAccountRepository>()
                .AddScoped<IAccountGroupRepository, AccountGroupRepository>()
                .AddScoped<IAccountHolderRepository, AccountHolderRepository>()
                .AddScoped<IBudgetRepository, BudgetRepository>()
                .AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>()
                .AddScoped<IReferenceDataRepository, ReferenceDataRepository>()
                .AddScoped<ISecurity, SecurityRepository>()
                .AddScoped<ITransactionExtraRepository, TransactionExtraRepository>()
                .AddScoped<ITransactionRepository, TransactionRepository>()
                .AddScoped<ITransactionTagRepository, TransactionTagRepository>()
                .AddScoped<ITransactionTagRuleRepository, TransactionTagRuleRepository>()
                .AddScoped<IVirtualAccountRepository, VirtualAccountRepository>();

    public static IServiceCollection AddEntities(this IServiceCollection services) =>
        services.AddAggregateRoots<MooBankContext>(typeof(IAccountGroupRepository).Assembly);
}
