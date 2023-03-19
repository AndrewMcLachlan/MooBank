using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Repositories.Ing;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Repositories.Ing;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMooBankDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankPlusContext>((services, options) => options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("MooBank"), options =>
        {
            options.EnableRetryOnFailure(3);
        }));


        return services.AddUnitOfWork<BankPlusContext>();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IInstitutionAccountRepository, InstitutionAccountRepository>()
                .AddScoped<IAccountHolderRepository, AccountHolderRepository>()
                .AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>()
                .AddScoped<IReferenceDataRepository, ReferenceDataRepository>()
                .AddScoped<ISecurityRepository, SecurityRepository>()
                .AddScoped<ITransactionExtraRepository, TransactionExtraRepository>()
                .AddScoped<ITransactionRepository, TransactionRepository>()
                .AddScoped<ITransactionTagRepository, TransactionTagRepository>()
                .AddScoped<ITransactionTagRuleRepository, TransactionTagRuleRepository>()
                .AddScoped<IVirtualAccountRepository, VirtualAccountRepository>();
}
