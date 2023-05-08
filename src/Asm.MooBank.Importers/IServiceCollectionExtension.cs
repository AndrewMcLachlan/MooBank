using Asm.MooBank.Importers;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddImporterFactory(this IServiceCollection services)
    {
        return services.AddTransient<IImporterFactory, ImporterFactory>();
    }
}
