using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Models;

/// <summary>
/// A general request parameters object that accepts an account ID.
/// </summary>
/// <param name="AccountId">The account ID.</param>
public record AccountRequest(Guid AccountId);

/// <summary>
/// A general request parameters object that accepts an account ID and some other GUID ID.
/// </summary>
/// <param name="AccountId">The account ID.</param>
/// <param name="Id">An ID for an object underneath account.</param>
public record AccountChildRequest<TChildId>(Guid AccountId, TChildId Id) where TChildId : struct;


public record AccountChildRequestWithBody<TChildId, TBody>(Guid AccountId,TChildId Id, [FromBody]TBody Body) where TChildId : struct;
