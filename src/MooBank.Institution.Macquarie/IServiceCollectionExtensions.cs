using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Macquarie.Domain;
using Asm.MooBank.Institution.Macquarie.Importers;
using Asm.MooBank.Institution.Macquarie.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Institution.Macquarie;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMacquarie(this IServiceCollection services)
    {
        MooBankContext.RegisterAssembly(typeof(IServiceCollectionExtensions).Assembly);

        return services.AddScoped<MacquarieImporter>()
              .AddScoped<ITransactionRawRepository, TransactionRawRepository>()
 .AddAggregateRoots<MooBankContext>(typeof(IServiceCollectionExtensions).Assembly);
    }
}
