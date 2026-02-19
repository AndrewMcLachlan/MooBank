using System.Reflection;
using Asm.AspNetCore.Modules;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Accounts;

public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Accounts().MapGroup(endpoints);
        new Endpoints.InstitutionAccounts().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));
        new Endpoints.RecurringEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy("accountId"));
        new Endpoints.VirtualRecurringEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy("accountId"));

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);
        services.AddValidatorsFromAssembly(Assembly);

        return services;
    }
}
