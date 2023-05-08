namespace Microsoft.Extensions.DependencyInjection;

public static class AsmMooBankQueriesIServiceCollectionExtensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services) =>
        services.AddQueryHandlers(typeof(AsmMooBankQueriesIServiceCollectionExtensions).Assembly);
}
