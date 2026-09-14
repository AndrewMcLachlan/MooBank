using System.Reflection;
using Asm.AspNetCore.Modules;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Budgets;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Budget().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.ReportEndpoint().MapGroup(endpoints).RequireAuthorization();

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(_assembly);
        services.AddQueryHandlers(_assembly);
        services.AddValidatorsFromAssembly(_assembly);

        services.AddScoped<Services.IBudgetGenerationReader, Services.BudgetGenerationReader>();

        return services;
    }
}
