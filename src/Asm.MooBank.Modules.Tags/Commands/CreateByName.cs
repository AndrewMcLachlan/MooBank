using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Tags.Commands;

public sealed record CreateByName(string Name, IEnumerable<int> Tags) : ICommand<Tag>
{
    public static async ValueTask<CreateByName?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        string name = httpContext.Request.RouteValues["name"] as string ?? throw new BadHttpRequestException("name not found");

        var tags = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<int>>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return new CreateByName(name, tags ?? []);
    }
}

internal sealed class CreateByNameHandler(IUnitOfWork unitOfWork, ITagRepository tagRepository, User user) : ICommandHandler<CreateByName, Tag>
{
    public async ValueTask<Tag> Handle(CreateByName request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out string name, out IEnumerable<int> tags);

        var tagEntities = await tagRepository.Get(tags, cancellationToken);

        Domain.Entities.Tag.Tag tag = new()
        {
            Name = name,
            FamilyId = user.FamilyId,
            Tags = [.. tagEntities],
            Settings = new()
            {
                ApplySmoothing = false,
                ExcludeFromReporting = false,
            }
        };
        tagRepository.Add(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
