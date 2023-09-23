using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Ing.Domain;
using Asm.MooBank.Institution.Ing.Importers;
using Asm.MooBank.Institution.Ing.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Institution.Ing;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddIng(this IServiceCollection services)
    {
        MooBankContext.RegisterAssembly(typeof(IServiceCollectionExtensions).Assembly);

        return services.AddScoped<IngImporter>()
                       .AddScoped<ITransactionRawRepository, TransactionRawRepository>()
                       .AddAggregateRoots<MooBankContext>(typeof(IServiceCollectionExtensions).Assembly);
    }
}
