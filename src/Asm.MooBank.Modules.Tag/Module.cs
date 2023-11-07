﻿using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Asm.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Modules.Tag;
public class Module : IModule
{
    private static readonly Assembly Assembly = typeof(Module).Assembly;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        new Endpoints.TagsEndpoints().MapGroup(endpoints);

        return endpoints;
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddCommandHandlers(Assembly);
        services.AddQueryHandlers(Assembly);

        return services;
    }
}