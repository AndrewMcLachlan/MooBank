using Asm.MooBank.Institution.Ing.Importers;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Institution.Ing;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddIng(this IServiceCollection services) =>
        services.AddScoped<IngImporter>()
                .AddQueryHandlers(typeof(IServiceCollectionExtensions).Assembly);

}
