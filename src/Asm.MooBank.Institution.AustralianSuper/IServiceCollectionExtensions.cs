using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.AustralianSuper.Domain;
using Asm.MooBank.Institution.AustralianSuper.Importers;
using Asm.MooBank.Institution.AustralianSuper.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Institution.AustralianSuper;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAustralianSuper(this IServiceCollection services)
    {
        MooBankContext.RegisterAssembly(typeof(IServiceCollectionExtensions).Assembly);

        return services.AddScoped<Importer>()
                       .AddScoped<ITransactionRawRepository, TransactionRawRepository>()
                       .AddAggregateRoots<MooBankContext>(typeof(IServiceCollectionExtensions).Assembly);
    }
}
