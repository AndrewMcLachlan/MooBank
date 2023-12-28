using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Institution.Commands;

public sealed record Update(int Id, string Name, int InstitutionTypeId) : ICommand<Models.Institution>
{
    public static async Task<Update> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        var id = (int)httpContext.Request.RouteValues["id"]!;

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { Id = id };
    }
}

internal class UpdateHandler(IInstitutionRepository repository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Update command, CancellationToken cancellationToken)
    {
        await Security.AssertAdministrator(cancellationToken);

        Domain.Entities.Institution.Institution entity = await repository.Get(command.Id, cancellationToken);

        entity.Name = command.Name;
        entity.InstitutionTypeId = command.InstitutionTypeId;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
