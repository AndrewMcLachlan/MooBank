using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record CreateByName(string Name, IEnumerable<int> Tags) : ICommand<MooBank.Models.Tag>
{
    public static async ValueTask<CreateByName?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        string name = httpContext.Request.RouteValues["name"] as string ?? throw new BadHttpRequestException("name not found");

        var tags = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<int>>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return new CreateByName(name, tags ?? []);
    }
}

internal sealed class CreateByNameHandler(IUnitOfWork unitOfWork, ITagRepository transactionTagRepository, AccountHolder accountHolder) : ICommandHandler<CreateByName, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(CreateByName request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out string name, out IEnumerable<int> tags);

        var tagEntities = await transactionTagRepository.Get(tags, cancellationToken);

        Domain.Entities.Tag.Tag tag = new()
        {
            Name = name,
            FamilyId = accountHolder.FamilyId,
            Tags = tagEntities.ToList(),
        };
        transactionTagRepository.Add(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
