﻿using System.Reflection;
using Asm.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Account;
public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.Accounts().MapGroup(endpoints);
        new Endpoints.Import().MapGroup(endpoints);
        new Endpoints.RecurringEndpoints().MapGroup(endpoints);
        new Endpoints.RulesEndpoints().MapGroup(endpoints);
        new Endpoints.VirtualAccounts().MapGroup(endpoints);

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);

        return services;
    }
}
