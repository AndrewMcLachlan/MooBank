using System.Reflection;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Bills;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Bills().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.BillAccounts().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());
        new Endpoints.BillReports().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.ChargeTypes().MapGroup(endpoints).RequireAuthorization();

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(_assembly);
        services.AddQueryHandlers(_assembly);
        services.AddValidatorsFromAssembly(_assembly);

        return services;
    }
}
