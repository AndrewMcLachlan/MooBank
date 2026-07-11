using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class TagFamilyAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationReader authorisationReader, User? user, IAuditLogger audit) : TolerantRouteAuthorisationHandler<TagFamilyRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "Tag";

    protected override async ValueTask<bool> IsAuthorised(object value) =>
        Int32.TryParse(value.ToString(), out var id) &&
        await authorisationReader.GetTagFamilyId(id, CancellationToken) == User!.FamilyId;
}
