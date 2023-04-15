using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Queries.Transactions;
using Microsoft.Identity.Client;

namespace Asm.MooBank.Web.Controllers;

[Route("api/accounts/{accountId}/[controller]")]
[ApiController]
[Authorize]
public class TransactionsController : CommandQueryController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService, IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<PagedResult<Transaction>>> Get(Guid accountId, int pageSize = 50, int pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string? filter = null, [FromQuery] int? tagId = null, [FromQuery] string? sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending, CancellationToken cancellationToken = default)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        var result = await GetTransactions(accountId, pageSize, pageNumber, start, end, filter, tagId, sortField, sortDirection, false, cancellationToken);

        return new ActionResult<PagedResult<Transaction>>(result);
    }

    [HttpGet("untagged/{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<PagedResult<Transaction>>> GetUntagged(Guid accountId, CancellationToken cancellationToken, int pageSize = 50, int pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string? filter = null, [FromQuery] int? tagId = null, [FromQuery] string? sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        var result = await GetTransactions(accountId, pageSize, pageNumber, start, end, filter, tagId, sortField, sortDirection, true, cancellationToken);

        return new ActionResult<PagedResult<Transaction>>(result);
    }

    [HttpPut("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> Add(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        return Created($"api/transactions/{id}/tag/{tagId}", await _transactionService.AddTransactionTag(accountId, id, tagId, cancellationToken));
    }

    [HttpDelete("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> RemoveTag(Guid accountId, Guid id, int tagId)
    {
        return await _transactionService.RemoveTransactionTag(accountId, id, tagId);
    }

    private Task<PagedResult<Transaction>> GetTransactions(Guid accountId, int pageSize, int pageNumber, DateTime? start, DateTime? end, string? filter, int? tagId, string? sortField, SortDirection sortDirection, bool untaggedOnly, CancellationToken cancellationToken)
    {
        GetTransactions request = new()
        {
            AccountId = accountId,
            Filter = filter,
            Start = start,
            End = end,
            TagId = tagId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortDirection = sortDirection,
            SortField = sortField,
            UntaggedOnly = untaggedOnly,
        };

        return QueryDispatcher.Dispatch(request, cancellationToken);
    }
}
