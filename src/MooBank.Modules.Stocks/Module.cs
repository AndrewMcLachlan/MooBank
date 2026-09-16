using System.Reflection;
using Asm.AspNetCore.Modules;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Stocks;

public class Module : IModule
{
    private static readonly Assembly _assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.StockHoldings().MapGroup(endpoints).RequireAuthorization();
        new Endpoints.StockTransactionsEndpoints().MapGroup(endpoints).RequireAuthorization(Policies.GetInstrumentViewerPolicy());

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
