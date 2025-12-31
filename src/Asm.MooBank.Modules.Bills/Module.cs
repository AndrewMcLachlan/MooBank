using System.Reflection;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Bills;

public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Bills().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.BillAccounts().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());
        new Endpoints.BillReports().MapGroup(endpoints).RequireAuthorization();

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);

        return services;
    }
}
