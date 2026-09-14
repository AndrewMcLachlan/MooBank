using System.Reflection;
using Asm.AspNetCore.Modules;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Users;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IServiceCollection AddServices(IServiceCollection services) =>
        services.AddCommandHandlers(_assembly)
                .AddQueryHandlers(_assembly)
                .AddValidatorsFromAssembly(_assembly);

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.User().MapGroup(endpoints).RequireAuthorization();
        return endpoints;
    }
}
