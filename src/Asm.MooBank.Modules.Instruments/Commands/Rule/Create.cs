﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Instruments.Commands.Rule;

public record Create(Guid AccountId, string Contains, string? Description, IEnumerable<MooBank.Models.Tag> Tags) : ICommand<Queries.Rules.Rule>
{
    public static async ValueTask<Create?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var create = await System.Text.Json.JsonSerializer.DeserializeAsync<Create>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return create! with { AccountId = accountId };
    }
}

internal class CreateHandler(IInstrumentRepository accountRepository, ITagRepository tagRepository, ISecurity security, IUnitOfWork unitOfWork) : ICommandHandler<Create, Queries.Rules.Rule>
{
    public async ValueTask<Queries.Rules.Rule> Handle(Create request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var account = await accountRepository.Get(request.AccountId, cancellationToken);

        var rule = new Domain.Entities.Instrument.Rule
        {
            InstrumentId = request.AccountId,
            Contains = request.Contains,
            Description = request.Description,
            Tags = (await tagRepository.Get(request.Tags.Select(t => t.Id), cancellationToken)).ToList(),
        };


        account.Rules.Add(rule);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
