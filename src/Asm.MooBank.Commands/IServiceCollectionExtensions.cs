namespace Microsoft.Extensions.DependencyInjection;

public static class AsmMooBankCommandsIServiceCollectionExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services) =>
        services.AddCommandHandlers(typeof(AsmMooBankCommandsIServiceCollectionExtensions).Assembly);
}
