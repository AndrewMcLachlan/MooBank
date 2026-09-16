using System.Reflection;
using Asm.AspNetCore.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.ReferenceData;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.ReferenceData().MapGroup(endpoints).RequireAuthorization();

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddQueryHandlers(_assembly);
        services.AddCommandHandlers(_assembly);
        services.AddValidatorsFromAssembly(_assembly);

        return services;
    }
}
