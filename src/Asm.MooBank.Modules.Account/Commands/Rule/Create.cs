﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Tag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record Create(Guid AccountId, string Contains, string? Description, IEnumerable<MooBank.Models.Tag> Tags) : ICommand<Models.Rule>
{
    public static async ValueTask<Create?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var create = await System.Text.Json.JsonSerializer.DeserializeAsync<Create>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return create! with { AccountId = accountId };
    }
}

internal class CreateHandler(IAccountRepository accountRepository, ITagRepository tagRepository, MooBank.Models.AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Rule>
{
    public async ValueTask<Models.Rule> Handle(Create request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await accountRepository.Get(request.AccountId, cancellationToken);

        var rule = new Domain.Entities.Account.Rule
        {
            AccountId = request.AccountId,
            Contains = request.Contains,
            Description = request.Description,
            Tags = (await tagRepository.Get(request.Tags.Select(t => t.Id), cancellationToken)).ToList(),
        };


        account.Rules.Add(rule);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
