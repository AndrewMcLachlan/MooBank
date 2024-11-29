using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Modules.Instruments.Models.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record Create(Guid InstrumentId, string Contains, string? Description, IEnumerable<MooBank.Models.Tag> Tags) : ICommand<Models.Rules.Rule>
{
    public static async ValueTask<Create?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["instrumentId"] as string, out Guid instrumentId)) throw new BadHttpRequestException("invalid instrument ID");

        var create = await System.Text.Json.JsonSerializer.DeserializeAsync<Create>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return create! with { InstrumentId = instrumentId };
    }
}

internal class CreateHandler(IInstrumentRepository instrumentRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Rules.Rule>
{
    public async ValueTask<Models.Rules.Rule> Handle(Create request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(request.InstrumentId, cancellationToken);

        var rule = new Domain.Entities.Instrument.Rule
        {
            InstrumentId = request.InstrumentId,
            Contains = request.Contains,
            Description = request.Description,
            Tags = (await tagRepository.Get(request.Tags.Select(t => t.Id), cancellationToken)).ToList(),
        };


        instrument.Rules.Add(rule);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
