﻿using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Institutions.Commands;

[DisplayName("UpdateInstitution")]
public sealed record Update(int Id, string Name, InstitutionType InstitutionType) : ICommand<Models.Institution>
{
    public static async Task<Update> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        var id = (int)httpContext.Request.RouteValues["id"]!;

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { Id = id };
    }
}

internal class UpdateHandler(IInstitutionRepository repository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Update command, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator(cancellationToken);

        Domain.Entities.Institution.Institution entity = await repository.Get(command.Id, cancellationToken);

        entity.Name = command.Name;
        entity.InstitutionType = command.InstitutionType;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
