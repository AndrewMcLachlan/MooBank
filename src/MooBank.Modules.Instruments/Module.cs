using System.Reflection;
using Asm.AspNetCore.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Instruments;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Instruments().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.Import().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());
        new Endpoints.RulesEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());
        new Endpoints.VirtualInstruments().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());
        new Endpoints.VirtualInstrumentRecurringEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(_assembly);
        services.AddQueryHandlers(_assembly);

        return services;
    }
}
