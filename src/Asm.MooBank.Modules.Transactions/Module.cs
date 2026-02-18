using System.Reflection;
using Asm.AspNetCore.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Transactions;

public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.TransactionsEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);

        return services;
    }
}
