using System.Reflection;
using Asm.AspNetCore.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Instruments;
public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Instruments().MapGroup(endpoints);
        new Endpoints.Import().MapGroup(endpoints).RequireAuthorization(Policies.InstrumentViewer);
        new Endpoints.RulesEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.InstrumentViewer);
        new Endpoints.VirtualAccounts().MapGroup(endpoints).RequireAuthorization(Policies.InstrumentViewer);

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);

        return services;
    }
}
