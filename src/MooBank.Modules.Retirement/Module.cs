using System.Reflection;
using Asm.AspNetCore.Modules;
using Asm.MooBank.Modules.Retirement.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Retirement;

public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.RetirementPlans().MapGroup(endpoints).RequireAuthorization();

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);
        services.AddValidatorsFromAssembly(Assembly);
        services.AddScoped<IRetirementProjectionEngine, RetirementProjectionEngine>();
        services.AddScoped<IMemberGuard, MemberGuard>();
        services.AddScoped<IPensionRateReader, PensionRateReader>();

        return services;
    }
}
