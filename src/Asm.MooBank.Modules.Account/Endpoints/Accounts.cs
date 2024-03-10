using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Commands.InstitutionAccount;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Modules.Account.Queries.Account;
using Asm.MooBank.Modules.Account.Queries.InstitutionAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;
internal class Accounts : EndpointGroupBase
{
    public override string Name => "Accounts";

    public override string Path => "/accounts";

    public override string Tags => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<InstitutionAccount>>("/")
            .WithNames("Get Accounts");

        builder.MapQuery<GetFormatted, AccountsList>("/position")
            .WithNames("Get Formatted Accounts List");

        builder.MapQuery<GetList, IEnumerable<ListItem<Guid>>>("/list")
            .WithNames("Get Accounts List");

        builder.MapQuery<Get, InstitutionAccount>("/{id}")
            .WithNames("Get Account");

        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Account", a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Account");

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}")
            .WithNames("Update Account");

        builder.MapPatchCommand<UpdateBalance, InstitutionAccount>("/{id}/balance")
            .WithNames("Set Balance");
    }

    internal static Delegate CreateCreateHandler<TRequest, TResult>(string routeName, Func<TResult, object> getRouteParams) where TRequest : ICommand<TResult>
    {
        return async ([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.Dispatch(request!, cancellationToken);

            return Results.CreatedAtRoute(routeName, getRouteParams(result), result);
        };
    }
}
/*

public static class Ext
{
    /// <summary>
    /// Map a request to a query.
    /// </summary>
    /// <typeparam name="TRequest">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapQuery<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : IQuery<TResponse> =>
        endpoints.MapGet(pattern, Handlers.HandleQuery<TRequest, TResponse>);

    /// <summary>
    /// Map a request to a query that returns results in pages..
    /// </summary>
    /// <typeparam name="TRequest">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPagedQuery<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : IQuery<PagedResult<TResponse>> =>
        endpoints.MapGet(pattern, Handlers.HandlePagedQuery<TRequest, TResponse>);

    /// <summary>
    /// Map a POST request to a command that creates a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="routeName">The name of the route that can be used to get the newly created resource.</param>
    /// <param name="getRouteParams">A delegate that creates the route parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPostCreate<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, string routeName, Func<TResponse, object> getRouteParams) where TRequest : ICommand<TResponse> =>
        endpoints.MapPost(pattern, Handlers.CreateCreateHandler<TRequest, TResponse>(routeName, getRouteParams));

    /// <summary>
    /// Map a POST request to a command that creates a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="routeName">The name of the route that can be used to get the newly created resource.</param>
    /// <param name="getRouteParams">A delegate that creates the route parameters.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPostCreate<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, string routeName, Func<TResponse, object> getRouteParams, CommandBinding binding) where TRequest : ICommand<TResponse> =>
        endpoints.MapPost(pattern, Handlers.CreateCreateHandler<TRequest, TResponse>(routeName, getRouteParams, binding));

    /// <summary>
    /// Map a PUT request to a command that creates a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="routeName">The name of the route that can be used to get the newly created resource.</param>
    /// <param name="getRouteParams">A delegate that creates the route parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPutCreate<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, string routeName, Func<TResponse, object> getRouteParams) where TRequest : ICommand<TResponse> =>
        endpoints.MapPut(pattern, Handlers.CreateCreateHandler<TRequest, TResponse>(routeName, getRouteParams));

    /// <summary>
    /// Map a PUT request to a command that creates a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="routeName">The name of the route that can be used to get the newly created resource.</param>
    /// <param name="getRouteParams">A delegate that creates the route parameters.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPutCreate<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, string routeName, Func<TResponse, object> getRouteParams, CommandBinding binding) where TRequest : ICommand<TResponse> =>
        endpoints.MapPut(pattern, Handlers.CreateCreateHandler<TRequest, TResponse>(routeName, getRouteParams, binding));

    /// <summary>
    /// Maps a request to a command to delete a resource and returns an empty response with code 204 - No Content.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapDelete<TRequest>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : ICommand =>
        endpoints.MapDelete(pattern, Handlers.HandleDelete<TRequest>);

    /// <summary>
    /// Maps a request to a command to delete a resource and return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapDelete<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : ICommand<TResponse> =>
        endpoints.MapDelete(pattern, Handlers.HandleDelete<TRequest, TResponse>);

    /// <summary>
    /// Maps a POST request to a command and returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : ICommand<TResponse> =>
        endpoints.MapPost(pattern, Handlers.HandleCommand<TRequest, TResponse>);

    /// <summary>
    /// Maps a POST request to a command and returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, CommandBinding binding) where TRequest : ICommand<TResponse> =>
        endpoints.MapPost(pattern, Handlers.CreateCommandHandler<TRequest, TResponse>(StatusCodes.Status200OK, binding));

    /// <summary>
    /// Maps a POST request to a command and returns an empty response with the given status code.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="returnStatusCode">The status code to return. Defaults to 204 - No Content.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapCommand<TRequest>(this IEndpointRouteBuilder endpoints, string pattern, int returnStatusCode) where TRequest : ICommand =>
        endpoints.MapPost(pattern, Handlers.CreateCommandHandler<TRequest>(returnStatusCode));

    /// <summary>
    /// Maps a POST request to a command and returns an empty response with the given status code.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="returnStatusCode">The status code to return. Defaults to 204 - No Content.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapCommand<TRequest>(this IEndpointRouteBuilder endpoints, string pattern, int returnStatusCode, CommandBinding binding) where TRequest : ICommand =>
        endpoints.MapPost(pattern, Handlers.CreateCommandHandler<TRequest>(returnStatusCode, binding));

    /// <summary>
    /// Maps a request to a command to patch a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPatchCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : ICommand<TResponse> =>
        endpoints.MapPatch(pattern, Handlers.HandleCommand<TRequest, TResponse>);

    /// <summary>
    /// Maps a request to a command to patch a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPatchCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, CommandBinding binding) where TRequest : ICommand<TResponse> =>
        endpoints.MapPatch(pattern, Handlers.CreateCommandHandler<TRequest, TResponse>(StatusCodes.Status200OK, binding));

    /// <summary>
    /// Maps a request to a command to put a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPutCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern) where TRequest : ICommand<TResponse> =>
        endpoints.MapPut(pattern, Handlers.HandleCommand<TRequest, TResponse>);

    /// <summary>
    /// Maps a request to a command to put a resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="binding">How the handler should bind parameters.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    public static RouteHandlerBuilder MapPutCommand<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, CommandBinding binding) where TRequest : ICommand<TResponse> =>
        endpoints.MapPut(pattern, Handlers.CreateCommandHandler<TRequest, TResponse>(StatusCodes.Status200OK, binding));
}



internal static class Handlers
{
    #region Integrated CQRS Handlers
    internal static async ValueTask<IResult> HandleQuery<TQuery, TResult>([AsParameters] TQuery query, IQueryDispatcher dispatcher, CancellationToken cancellationToken) where TQuery : IQuery<TResult> =>
       Results.Ok(await dispatcher.Dispatch(query, cancellationToken));

    internal static async ValueTask<IResult> HandlePagedQuery<TQuery, TResult>([AsParameters] TQuery query, HttpContext http, IQueryDispatcher dispatcher, CancellationToken cancellationToken) where TQuery : IQuery<PagedResult<TResult>>
    {
        PagedResult<TResult> result = await dispatcher.Dispatch(query, cancellationToken);

        http.Response.Headers.Append("X-Total-Count", result.Total.ToString());
        return Results.Ok(result.Results);
    }

    internal static async ValueTask<IResult> HandleDelete<TRequest>([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : ICommand
    {
        await dispatcher.Dispatch(request!, cancellationToken);

        return Results.NoContent();
    }

    internal static async ValueTask<IResult> HandleDelete<TRequest, TResult>([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : ICommand<TResult>
    {
        var result = await dispatcher.Dispatch(request!, cancellationToken);

        return Results.Ok(result);
    }

    internal static ValueTask<TResult> HandleCommand<TRequest, TResult>([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : ICommand<TResult> =>
       dispatcher.Dispatch(request!, cancellationToken);

    internal static ValueTask HandleCommand<TRequest>([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : ICommand =>
       dispatcher.Dispatch(request!, cancellationToken);
    #endregion

    #region Advanced CQRS Handlers
    internal static Delegate CreateCreateHandler<TRequest, TResult>(string routeName, Func<TResult, object> getRouteParams, CommandBinding binding = CommandBinding.None) where TRequest : ICommand<TResult>
    {
        return ParameterBinding<TRequest, TResult>(
            async (TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.Dispatch(request!, cancellationToken);

                return Results.CreatedAtRoute(routeName, getRouteParams(result), result);
            },
            binding
        );
    }

    internal static Delegate CreateCommandHandler<TRequest>(Func<TRequest, object> createCommand)
    {
        return async (TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            return await dispatcher.Dispatch(createCommand(request), cancellationToken);
        };
    }

    internal static Delegate CreateCommandHandler<TRequest>(int returnStatusCode, CommandBinding binding = CommandBinding.None) where TRequest : ICommand
    {
        return ParameterBinding(
            async (TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                await dispatcher.Dispatch(request, cancellationToken);
                return Results.StatusCode(returnStatusCode);
            },
            binding
        );

    }

    internal static Delegate CreateCommandHandler<TRequest, TResponse>(int returnStatusCode, CommandBinding binding = CommandBinding.None) where TRequest : ICommand<TResponse>
    {
        return ParameterBinding<TRequest, TResponse>(
            async (TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                await dispatcher.Dispatch(request, cancellationToken);
                return Results.StatusCode(returnStatusCode);
            },
            binding
        );

    }

    internal static Delegate CreateDeleteHandler<TRequest>(Func<TRequest, object> func) =>
       async ([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
       {
           await dispatcher.Dispatch(func(request), cancellationToken);

           return Results.NoContent();
       };
    #endregion


    private static Delegate ParameterBinding<TRequest>(Func<TRequest, ICommandDispatcher, CancellationToken, Task<IResult>> func, CommandBinding binding) where TRequest : ICommand
    {
        return binding switch
        {
            CommandBinding.None => func,
            CommandBinding.Body => ([FromBody] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) => func(request, dispatcher, cancellationToken),
            CommandBinding.Parameters => ([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) => func(request, dispatcher, cancellationToken),
            _ => func
        };
    }

    private static Delegate ParameterBinding<TRequest, TResponse>(Func<TRequest, ICommandDispatcher, CancellationToken, Task<IResult>> func, CommandBinding binding) where TRequest : ICommand<TResponse>
    {
        return binding switch
        {
            CommandBinding.None => func,
            CommandBinding.Body => ([FromBody] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) => func(request, dispatcher, cancellationToken),
            CommandBinding.Parameters => ([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) => func(request, dispatcher, cancellationToken),
            _ => func
        };
    }
}

public enum CommandBinding
{
    /// <summary>
    /// No parameter binding specified.
    /// </summary>
    /// <remarks>
    /// Unless a BindAsync or TryParse method is defined, body binding will be used.
    /// </remarks>
    None = 0,
    /// <summary>
    /// Use body binding with the <see cref="Microsoft.AspNetCore.Mvc.FromBodyAttribute"/>.
    /// </summary>
    Body = 1,
    /// <summary>
    /// Use parameter binding with the <see cref="AsParametersAttribute"/>.
    /// </summary>
    Parameters = 2,
}*/
